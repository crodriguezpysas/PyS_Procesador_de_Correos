namespace ProcesadorCorreosPYS.Domain.Models;

public sealed record ProcessingLogEntry(DateTimeOffset Timestamp, string Tag, string Message, bool Automatic);
