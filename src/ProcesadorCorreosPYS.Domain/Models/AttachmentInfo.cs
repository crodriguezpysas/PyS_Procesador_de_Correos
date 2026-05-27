namespace ProcesadorCorreosPYS.Domain.Models;

public sealed record AttachmentInfo(
    string FileName,
    long SizeBytes,
    bool IsInline = false,
    string? ContentId = null,
    byte[]? Content = null);
