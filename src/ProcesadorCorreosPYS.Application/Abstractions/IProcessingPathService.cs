using ProcesadorCorreosPYS.Application.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IProcessingPathService
{
    ProcessingPaths Build(DateOnly date);
}
