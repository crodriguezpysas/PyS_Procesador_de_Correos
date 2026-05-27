using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Application.Services;
using ProcesadorCorreosPYS.Infrastructure.Services;

namespace ProcesadorCorreosPYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OAuthOptions>().Bind(configuration.GetSection("OAuth"));
        services.AddOptions<ProcessingOptions>().Bind(configuration.GetSection("Processing"));

        services.AddSingleton<IUniqueIdService, UniqueIdService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileNameSanitizer, FileNameSanitizer>();
        services.AddSingleton<IFolderRangeParser, FolderRangeParser>();
        services.AddSingleton<IStateStore, FileStateStore>();
        services.AddSingleton<ICsvService, CsvService>();
        services.AddSingleton<IExcelService, ExcelService>();

        services.AddSingleton<IProcessingPathService, ProcessingPathService>();
        services.AddSingleton<IEmailClient, EwsEmailClient>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IStickerService, StickerService>();
        services.AddSingleton<IFileCopyService, FileCopyService>();
        services.AddSingleton<IEmailProcessorService, EmailProcessorService>();
        services.AddSingleton<IAutomaticProcessorService, AutomaticProcessorService>();
        services.AddSingleton<ILogSink, NullLogSink>();

        return services;
    }
}
