using System.Security.Cryptography;
using System.Text;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class UniqueIdService : IUniqueIdService
{
    public string CreateSecondaryUniqueId(EmailMessage email)
    {
        var attachments = string.Join(";", email.Attachments.Select(a => a.FileName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        var source = $"{email.ReceivedAt:yyyy-MM-dd HH:mm:ss}|{email.Subject}|{attachments}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(hash)[..16];
    }
}
