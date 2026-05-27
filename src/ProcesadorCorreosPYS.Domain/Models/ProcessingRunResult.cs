namespace ProcesadorCorreosPYS.Domain.Models;

public sealed class ProcessingRunResult
{
    public int TotalFetched { get; set; }
    public int Processed { get; set; }
    public int Duplicates { get; set; }
    public int PendingReview { get; set; }
    public List<ProcessedEmailRecord> Records { get; } = [];
}
