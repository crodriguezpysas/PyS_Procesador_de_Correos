using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Infrastructure;
using ProcesadorCorreosPYS.Wpf.Logging;
using ProcesadorCorreosPYS.Wpf.ViewModels;
using Serilog;

namespace ProcesadorCorreosPYS.Wpf;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddJsonFile("appsettings.example.json", optional: true)
                      .AddEnvironmentVariables();
            })
            .UseSerilog((context, _, loggerConfig) =>
            {
                var path = context.Configuration["Logging:FilePath"] ?? "logs/pys-procesador-.log";
                loggerConfig.WriteTo.File(path, rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);
                services.AddSingleton<ObservableLogSink>();
                services.AddSingleton<ILogSink>(sp => sp.GetRequiredService<ObservableLogSink>());
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        window.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
