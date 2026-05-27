using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Application.Models;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class ProcessingPathService(IOptions<ProcessingOptions> options) : IProcessingPathService
{
    private readonly ProcessingOptions _options = options.Value;

    public ProcessingPaths Build(DateOnly date)
    {
        var yyMM = date.ToString("yyMM");
        var yyMMdd = date.ToString("yyMMdd");
        var yyyyMMdd = date.ToString("yyyyMMdd");

        var dayFolder = Path.Combine(_options.BaseFolder, yyMM, yyMMdd);
        var reportFolder = Path.Combine(_options.BaseFolder, "Reports", yyyyMMdd);

        return new ProcessingPaths
        {
            DayFolder = dayFolder,
            ReportFolder = reportFolder,
            ProcessedIdsPath = Path.Combine(reportFolder, "processed_ids.txt"),
            LastProcessedPath = Path.Combine(reportFolder, $"lastProcessedState_{yyyyMMdd}.txt"),
            ConsecutiveStatePath = Path.Combine(reportFolder, "consecutivo_state.json"),
            SummaryCsvPath = Path.Combine(reportFolder, $"summary_{yyyyMMdd}.csv"),
            SummaryExcelPath = Path.Combine(reportFolder, $"summary_{yyyyMMdd}.xlsx"),
            AlternativeExcelPath = Path.Combine(reportFolder, $"Automatico_{yyMMdd}_01.xlsx")
        };
    }
}
