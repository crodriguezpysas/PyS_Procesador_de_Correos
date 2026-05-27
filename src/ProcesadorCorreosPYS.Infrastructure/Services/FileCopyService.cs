using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class FileCopyService(IOptions<ProcessingOptions> options) : IFileCopyService
{
    private readonly ProcessingOptions _options = options.Value;

    public ValueTask<int> CopyDailyOutputsAsync(DateOnly date, string reportsFolder, string dayFolder, CancellationToken cancellationToken = default)
    {
        var target = Path.Combine(_options.BcsFolder, date.ToString("yyMM"), date.ToString("yyMMdd"));
        Directory.CreateDirectory(target);

        var copied = 0;
        foreach (var file in Directory.EnumerateFiles(dayFolder, "PS*.pdf", SearchOption.AllDirectories)
                     .Concat(Directory.EnumerateFiles(reportsFolder, "*.xlsx", SearchOption.TopDirectoryOnly)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destination = Path.Combine(target, Path.GetFileName(file));
            if (File.Exists(destination) && new FileInfo(destination).Length == new FileInfo(file).Length)
            {
                continue;
            }

            File.Copy(file, destination, overwrite: true);
            copied++;
        }

        return ValueTask.FromResult(copied);
    }
}
