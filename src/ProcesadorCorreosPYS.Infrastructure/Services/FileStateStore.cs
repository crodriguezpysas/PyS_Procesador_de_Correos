using ProcesadorCorreosPYS.Application.Abstractions;

namespace ProcesadorCorreosPYS.Infrastructure.Services;

public sealed class FileStateStore : IStateStore
{
    public async ValueTask SaveLastProcessedIdAsync(string path, string value, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, value, cancellationToken);
    }

    public async ValueTask<string?> LoadLastProcessedIdAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }
}
