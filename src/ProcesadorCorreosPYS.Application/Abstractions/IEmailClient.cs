using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Abstractions;

public interface IEmailClient
{
    ValueTask<IReadOnlyCollection<EmailMessage>> FetchEmailsAsync(EmailQuery query, CancellationToken cancellationToken = default);
}
