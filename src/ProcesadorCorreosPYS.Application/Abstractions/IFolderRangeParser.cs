namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IFolderRangeParser
{
    IReadOnlyList<int> Parse(string input);
}
