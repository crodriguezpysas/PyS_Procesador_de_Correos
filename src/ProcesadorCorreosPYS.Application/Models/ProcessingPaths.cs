namespace ProcesadorCorreosPYS.Application.Models;

public sealed class ProcessingPaths
{
    public required string DayFolder { get; init; }
    public required string ReportFolder { get; init; }
    public required string ProcessedIdsPath { get; init; }
    public required string LastProcessedPath { get; init; }
    public required string ConsecutiveStatePath { get; init; }
    public required string SummaryCsvPath { get; init; }
    public required string SummaryExcelPath { get; init; }
    public required string AlternativeExcelPath { get; init; }
}
