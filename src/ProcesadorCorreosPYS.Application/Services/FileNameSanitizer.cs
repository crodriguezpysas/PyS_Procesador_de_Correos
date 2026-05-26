using ProcesadorCorreosPYS.Application.Abstractions;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class FileNameSanitizer : IFileNameSanitizer
{
    private static readonly char[] InvalidChars = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    public string Sanitize(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "adjunto_sin_nombre";
        }

        var sanitized = fileName.Trim();
        foreach (var invalidChar in InvalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        return sanitized.Length == 0 ? "adjunto_sin_nombre" : sanitized;
    }
}
