namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class ProcessedEmailRecord
{
    public required DateOnly Fecha { get; init; }
    public required string Sticker { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required int AttachmentCount { get; init; }
    public required int EmptyAttachmentCount { get; init; }
    public required string AttachmentIncidents { get; init; }
    public required string PdfPath { get; init; }
    public required ProcessingStatus Status { get; init; }
    public required string MessageId { get; init; }
    public required string UniqueId { get; init; }
    public required int Consecutivo { get; init; }
}
