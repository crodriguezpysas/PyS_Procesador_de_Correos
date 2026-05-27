using ProcesadorCorreosPYS.Application.Services;

namespace ProcesadorCorreosPYS.Tests.Application;

public class FolderRangeParserTests
{
    [Fact]
    public void Parse_SupportsRangeAndList()
    {
        var parser = new FolderRangeParser();

        var values = parser.Parse("1-3,5,7-6");

        Assert.Equal([1, 2, 3, 5, 6, 7], values);
    }
}
