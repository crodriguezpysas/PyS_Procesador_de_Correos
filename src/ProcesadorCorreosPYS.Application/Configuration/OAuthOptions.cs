namespace ProcesadorCorreosPYS.Application.Configuration;

public sealed class OAuthOptions
{
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string? ClientSecret { get; init; }
    public bool UseDelegated { get; init; }
    public string EwsUrl { get; init; } = "https://outlook.office365.com/EWS/Exchange.asmx";
    public string Mailbox { get; init; } = string.Empty;
}
