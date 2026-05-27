namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IFileNameSanitizer
{
    string Sanitize(string fileName);
}
