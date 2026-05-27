using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Application.Services;
using ProcesadorCorreosPYS.Domain.Models;
using ProcesadorCorreosPYS.Infrastructure.Services;

namespace ProcesadorCorreosPYS.Tests.Application;

public sealed class EmailProcessorServiceTests
{
    [Fact]
    public async Task ProcessManual_DetectsPrimaryDuplicatesAndPersistsFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "pys_processor_tests", Guid.NewGuid().ToString("N"));
        var options = Options.Create(new ProcessingOptions { BaseFolder = root, BcsFolder = Path.Combine(root, "BCS") });
        var date = new DateOnly(2026, 5, 27);

        var service = new EmailProcessorService(
            new FakeEmailClient([
                CreateEmail("id-1", date),
                CreateEmail("id-1", date)
            ]),
            new UniqueIdService(),
            new FileNameSanitizer(),
            new FileStateStore(),
            new ProcessingPathService(options),
            new FakePdfService(),
            new FakeStickerService(),
            new FakeCsvService(),
            new FakeExcelService(),
            new FileCopyService(options),
            new NullLogSink(),
            options,
            NullLogger<EmailProcessorService>.Instance);

        var result = await service.ProcessManualAsync(date);

        Assert.Equal(2, result.TotalFetched);
        Assert.Equal(1, result.Processed);
        Assert.Equal(1, result.Duplicates);

        var expectedFolder = Path.Combine(root, "2605", "260527", "0001");
        Assert.True(File.Exists(Path.Combine(expectedFolder, "0.html")));
        Assert.True(File.Exists(Path.Combine(expectedFolder, "0.pdf")));
        Assert.True(File.Exists(Path.Combine(expectedFolder, "metadata.json")));
    }

    private static EmailMessage CreateEmail(string id, DateOnly date)
    {
        return new EmailMessage
        {
            InternetMessageId = id,
            ReceivedAt = date.ToDateTime(TimeOnly.MinValue),
            From = "a@contoso.com",
            To = "b@contoso.com",
            Subject = "Asunto",
            Body = "Cuerpo",
            IsHtmlBody = false,
            Attachments = []
        };
    }

    private sealed class FakeEmailClient(IReadOnlyCollection<EmailMessage> emails) : IEmailClient
    {
        public ValueTask<IReadOnlyCollection<EmailMessage>> FetchEmailsAsync(EmailQuery query, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(emails);
    }

    private sealed class FakePdfService : IPdfService
    {
        public async ValueTask<string> GeneratePdfFromHtmlAsync(string htmlPath, string outputPdfPath, CancellationToken cancellationToken = default)
        {
            await File.WriteAllBytesAsync(outputPdfPath, Enumerable.Repeat((byte)65, 512).ToArray(), cancellationToken);
            return outputPdfPath;
        }
    }

    private sealed class FakeStickerService : IStickerService
    {
        public async ValueTask<string> GenerateStickerAsync(string stickerText, string outputPdfPath, CancellationToken cancellationToken = default)
        {
            await File.WriteAllBytesAsync(outputPdfPath, Enumerable.Repeat((byte)66, 256).ToArray(), cancellationToken);
            return outputPdfPath;
        }
    }

    private sealed class FakeCsvService : ICsvService
    {
        public ValueTask ExportIncrementalAsync(string csvPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    private sealed class FakeExcelService : IExcelService
    {
        public ValueTask ExportResumenIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask ExportAlternativoIncrementalAsync(string excelPath, IReadOnlyCollection<ProcessedEmailRecord> rows, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
