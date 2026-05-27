using ProcesadorCorreosPYS.Application.Services;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Tests.Application;

public class UniqueIdServiceTests
{
    [Fact]
    public void CreateSecondaryUniqueId_Returns16HexChars()
    {
        var service = new UniqueIdService();
        var email = new EmailMessage
        {
            InternetMessageId = "id-1",
            ReceivedAt = new DateTimeOffset(2026, 5, 26, 10, 30, 0, TimeSpan.Zero),
            From = "a@contoso.com",
            To = "b@contoso.com",
            Subject = "Prueba",
            Body = "Hola",
            Attachments = [new AttachmentInfo("a.pdf", 10)]
        };

        var uniqueId = service.CreateSecondaryUniqueId(email);

        Assert.Matches("^[0-9A-F]{16}$", uniqueId);
    }
}
