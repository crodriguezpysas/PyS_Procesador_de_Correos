using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class EwsEmailClient(IOptions<OAuthOptions> oauthOptions, ILogger<EwsEmailClient> logger) : IEmailClient
{
    private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Msg = "http://schemas.microsoft.com/exchange/services/2006/messages";
    private static readonly XNamespace Types = "http://schemas.microsoft.com/exchange/services/2006/types";

    private readonly OAuthOptions _options = oauthOptions.Value;

    public async ValueTask<IReadOnlyCollection<EmailMessage>> FetchEmailsAsync(EmailQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.TenantId))
        {
            logger.LogWarning("OAuth no configurado, se omite consulta EWS");
            return [];
        }

        var token = await AcquireAccessTokenAsync(cancellationToken);
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = BuildFindItemRequest(query);
        using var content = new StringContent(request, Encoding.UTF8, "text/xml");
        using var response = await httpClient.PostAsync(_options.EwsUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseFindItemResponse(xml, query);
    }

    private string BuildFindItemRequest(EmailQuery query)
    {
        var mailboxXml = string.IsNullOrWhiteSpace(_options.Mailbox)
            ? string.Empty
            : $"<t:Mailbox><t:EmailAddress>{Escape(_options.Mailbox)}</t:EmailAddress></t:Mailbox>";

        return $"""
               <?xml version="1.0" encoding="utf-8"?>
               <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                              xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types"
                              xmlns:m="http://schemas.microsoft.com/exchange/services/2006/messages">
                 <soap:Body>
                   <m:FindItem Traversal="Shallow">
                     <m:ItemShape>
                       <t:BaseShape>Default</t:BaseShape>
                       <t:AdditionalProperties>
                         <t:FieldURI FieldURI="item:InternetMessageId"/>
                         <t:FieldURI FieldURI="item:DateTimeReceived"/>
                         <t:FieldURI FieldURI="item:Subject"/>
                         <t:FieldURI FieldURI="message:From"/>
                         <t:FieldURI FieldURI="message:ToRecipients"/>
                         <t:FieldURI FieldURI="item:Body"/>
                       </t:AdditionalProperties>
                     </m:ItemShape>
                     <m:IndexedPageItemView MaxEntriesReturned="{query.MaxItems}" Offset="0" BasePoint="Beginning"/>
                     <m:Restriction>
                       <t:And>
                         <t:IsGreaterThanOrEqualTo>
                           <t:FieldURI FieldURI="item:DateTimeReceived"/>
                           <t:FieldURIOrConstant><t:Constant Value="{query.Start:yyyy-MM-ddTHH:mm:ss}"/></t:FieldURIOrConstant>
                         </t:IsGreaterThanOrEqualTo>
                         <t:IsLessThanOrEqualTo>
                           <t:FieldURI FieldURI="item:DateTimeReceived"/>
                           <t:FieldURIOrConstant><t:Constant Value="{query.End:yyyy-MM-ddTHH:mm:ss}"/></t:FieldURIOrConstant>
                         </t:IsLessThanOrEqualTo>
                       </t:And>
                     </m:Restriction>
                     <m:ParentFolderIds>
                       <t:DistinguishedFolderId Id="inbox">
                         {mailboxXml}
                       </t:DistinguishedFolderId>
                     </m:ParentFolderIds>
                   </m:FindItem>
                 </soap:Body>
               </soap:Envelope>
               """;
    }

    private IReadOnlyCollection<EmailMessage> ParseFindItemResponse(string xml, EmailQuery query)
    {
        var document = XDocument.Parse(xml);
        var items = document.Descendants(Types + "Message");
        var result = new List<EmailMessage>();

        foreach (var item in items)
        {
            var messageId = item.Element(Types + "InternetMessageId")?.Value ?? Guid.NewGuid().ToString("N");
            if (!string.IsNullOrWhiteSpace(query.LastProcessedMessageId) &&
                string.Equals(query.LastProcessedMessageId, messageId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var receivedAt = DateTimeOffset.TryParse(item.Element(Types + "DateTimeReceived")?.Value, out var parsed)
                ? parsed
                : DateTimeOffset.Now;

            var toRecipients = string.Join(';', item.Descendants(Types + "Mailbox").Select(x => x.Element(Types + "EmailAddress")?.Value).Where(x => !string.IsNullOrWhiteSpace(x))!);
            var bodyElement = item.Element(Types + "Body");
            var bodyType = bodyElement?.Attribute("BodyType")?.Value;

            result.Add(new EmailMessage
            {
                InternetMessageId = messageId,
                ReceivedAt = receivedAt,
                From = item.Element(Types + "From")?.Descendants(Types + "EmailAddress").FirstOrDefault()?.Value ?? string.Empty,
                To = toRecipients,
                Subject = item.Element(Types + "Subject")?.Value ?? string.Empty,
                Body = bodyElement?.Value ?? string.Empty,
                IsHtmlBody = string.Equals(bodyType, "HTML", StringComparison.OrdinalIgnoreCase),
                Attachments = []
            });
        }

        return result;
    }

    private async Task<string> AcquireAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_options.UseDelegated)
        {
            var publicClient = PublicClientApplicationBuilder
                .Create(_options.ClientId)
                .WithTenantId(_options.TenantId)
                .WithRedirectUri("http://localhost")
                .Build();

            var result = await publicClient.AcquireTokenInteractive(["https://outlook.office365.com/EWS.AccessAsUser.All"])
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync(cancellationToken);

            return result.AccessToken;
        }

        var secret = Environment.GetEnvironmentVariable("OAuth__ClientSecret") ?? _options.ClientSecret;
        if (string.IsNullOrWhiteSpace(secret) || string.Equals(secret, "USE_ENV_VAR_ONLY", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OAuth client secret no configurado. Use OAuth__ClientSecret.");
        }

        var confidentialClient = ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithTenantId(_options.TenantId)
            .WithClientSecret(secret)
            .Build();

        var authResult = await confidentialClient.AcquireTokenForClient(["https://outlook.office365.com/.default"]).ExecuteAsync(cancellationToken);
        return authResult.AccessToken;
    }

    private static string Escape(string value)
        => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
