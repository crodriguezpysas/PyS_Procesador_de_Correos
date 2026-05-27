using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Application.Services;

namespace ProcesadorCorreosPYS.Tests.Application;

public sealed class ProcessingPathServiceTests
{
    [Fact]
    public void Build_GeneratesExpectedHierarchy()
    {
        var options = Options.Create(new ProcessingOptions { BaseFolder = "C:/Emails" });
        var service = new ProcessingPathService(options);

        var result = service.Build(new DateOnly(2026, 5, 27));

        Assert.Equal(Path.Combine("C:/Emails", "2605", "260527"), result.DayFolder);
        Assert.Equal(Path.Combine("C:/Emails", "Reports", "20260527"), result.ReportFolder);
        Assert.Contains("processed_ids.txt", result.ProcessedIdsPath);
    }
}
