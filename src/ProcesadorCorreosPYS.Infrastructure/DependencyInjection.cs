using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Services;
using ProcesadorCorreosPYS.Infrastructure.Services;

namespace ProcesadorCorreosPYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IUniqueIdService, UniqueIdService>();
        services.AddSingleton<IFileNameSanitizer, FileNameSanitizer>();
        services.AddSingleton<IFolderRangeParser, FolderRangeParser>();
        services.AddSingleton<IStateStore, FileStateStore>();
        services.AddSingleton<ICsvService, CsvService>();
        services.AddSingleton<IExcelService, ExcelService>();
        return services;
    }
}
