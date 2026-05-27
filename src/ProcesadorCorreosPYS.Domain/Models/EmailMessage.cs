namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class EmailMessage
{
    public required string InternetMessageId { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtmlBody { get; init; }
    public IReadOnlyList<AttachmentInfo> Attachments { get; init; } = [];
}
