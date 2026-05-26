using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IUniqueIdService
{
    string CreateSecondaryUniqueId(EmailMessage email);
}
