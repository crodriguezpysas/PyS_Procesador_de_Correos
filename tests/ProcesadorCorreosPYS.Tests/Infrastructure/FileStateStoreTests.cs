using ProcesadorCorreosPYS.Infrastructure.Services;

namespace ProcesadorCorreosPYS.Tests.Infrastructure;

public class FileStateStoreTests
{
    [Fact]
    public async Task SaveAndLoad_Roundtrip()
    {
        var store = new FileStateStore();
        var dir = Path.Combine(Path.GetTempPath(), "pys_tests", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(dir, "state.txt");

        await store.SaveLastProcessedIdAsync(file, "abc123");
        var value = await store.LoadLastProcessedIdAsync(file);

        Assert.Equal("abc123", value);
    }
}
