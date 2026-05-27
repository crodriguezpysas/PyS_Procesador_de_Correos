namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IStateStore
{
    ValueTask SaveLastProcessedIdAsync(string path, string value, CancellationToken cancellationToken = default);
    ValueTask<string?> LoadLastProcessedIdAsync(string path, CancellationToken cancellationToken = default);
}
