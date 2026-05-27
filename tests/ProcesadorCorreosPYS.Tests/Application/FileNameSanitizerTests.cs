using ProcesadorCorreosPYS.Application.Services;

namespace ProcesadorCorreosPYS.Tests.Application;

public class FileNameSanitizerTests
{
    [Fact]
    public void Sanitize_ReplacesInvalidCharacters()
    {
        var service = new FileNameSanitizer();

        var sanitized = service.Sanitize("ab:c?.pdf");

        Assert.DoesNotContain(':', sanitized);
        Assert.DoesNotContain('?', sanitized);
    }
}
