using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Application.Configuration;
using ProcesadorCorreosPYS.Domain.Models;
using ProcesadorCorreosPYS.Wpf.Commands;
using ProcesadorCorreosPYS.Wpf.Logging;

namespace ProcesadorCorreosPYS.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IEmailProcessorService _emailProcessor;
    private readonly IAutomaticProcessorService _automaticProcessor;
    private readonly IFolderRangeParser _rangeParser;
    private readonly ProcessingOptions _processingOptions;

    private DateTime _fechaObjetivo = DateTime.Today;
    private string _rangoSubcarpetas = "1-5";
    private string _offlineFolder = string.Empty;
    private string _estadoAutomatico = "Inicializado";

    public MainViewModel(
        IEmailProcessorService emailProcessor,
        IAutomaticProcessorService automaticProcessor,
        IFolderRangeParser rangeParser,
        IOptions<ProcessingOptions> processingOptions,
        ObservableLogSink observableLogSink)
    {
        _emailProcessor = emailProcessor;
        _automaticProcessor = automaticProcessor;
        _rangeParser = rangeParser;
        _processingOptions = processingOptions.Value;

        observableLogSink.EntryPublished += OnLogPublished;

        ProcesarAhoraCommand = new AsyncRelayCommand(ProcesarAhoraAsync);
        LimpiarCommand = new RelayCommand(() => ManualLogs.Clear());
        GenerarStickersCommand = new AsyncRelayCommand(GenerarStickersAsync);
        CrearExcelCommand = new AsyncRelayCommand(CrearExcelAsync);
        ExtraerCorreosCommand = new AsyncRelayCommand(ExtraerCorreosAsync);
        EjecutarCicloAutomaticoCommand = new AsyncRelayCommand(EjecutarCicloAutomaticoAsync);

        PublishInfo(false, "Sistema listo en modo manual.");
        PublishInfo(true, "Modo automático en espera de horario configurado.");
    }

    public DateTime FechaObjetivo
    {
        get => _fechaObjetivo;
        set => SetProperty(ref _fechaObjetivo, value);
    }

    public string RangoSubcarpetas
    {
        get => _rangoSubcarpetas;
        set => SetProperty(ref _rangoSubcarpetas, value);
    }

    public string OfflineFolder
    {
        get => _offlineFolder;
        set => SetProperty(ref _offlineFolder, value);
    }

    public string EstadoAutomatico
    {
        get => _estadoAutomatico;
        set => SetProperty(ref _estadoAutomatico, value);
    }

    public string VentanaHoraria => $"{_processingOptions.WindowStart} - {_processingOptions.WindowEnd}";

    public ObservableCollection<string> ManualLogs { get; } = [];
    public ObservableCollection<string> AutomaticoLogs { get; } = [];
    public ObservableCollection<ProcessedEmailRecord> Registros { get; } = [];

    public ICommand ProcesarAhoraCommand { get; }
    public ICommand LimpiarCommand { get; }
    public ICommand GenerarStickersCommand { get; }
    public ICommand CrearExcelCommand { get; }
    public ICommand ExtraerCorreosCommand { get; }
    public ICommand EjecutarCicloAutomaticoCommand { get; }

    private async Task ProcesarAhoraAsync()
    {
        var result = await _emailProcessor.ProcessManualAsync(DateOnly.FromDateTime(FechaObjetivo));
        LoadRecords(result.Records);
        PublishInfo(false, $"Procesados={result.Processed} Duplicados={result.Duplicates} PendienteRevision={result.PendingReview}");
    }

    private async Task ExtraerCorreosAsync()
    {
        var folder = string.IsNullOrWhiteSpace(OfflineFolder)
            ? Path.Combine(_processingOptions.BaseFolder, FechaObjetivo.ToString("yyMM"), FechaObjetivo.ToString("yyMMdd"))
            : OfflineFolder;

        var records = await _emailProcessor.ExtractFromDownloadedFolderAsync(folder, DateOnly.FromDateTime(FechaObjetivo));
        LoadRecords(records);
        PublishInfo(false, $"Extracción offline completada: {records.Count} registros");
    }

    private async Task GenerarStickersAsync()
    {
        var request = new StickerGenerationRequest
        {
            Date = DateOnly.FromDateTime(FechaObjetivo),
            FolderNumbers = _rangeParser.Parse(RangoSubcarpetas)
        };

        var generated = await _emailProcessor.GenerateStickersManualAsync(request);
        PublishInfo(false, $"Stickers generados: {generated.Count}");
    }

    private async Task CrearExcelAsync()
    {
        await _emailProcessor.GenerateExcelManualAsync(DateOnly.FromDateTime(FechaObjetivo));
        PublishInfo(false, "Excel/CSV incremental actualizado");
    }

    private async Task EjecutarCicloAutomaticoAsync()
    {
        EstadoAutomatico = "Ejecutando ciclo...";
        var result = await _automaticProcessor.ExecuteCycleAsync(DateOnly.FromDateTime(FechaObjetivo));
        EstadoAutomatico = result is null ? "Fuera de horario" : $"Último ciclo OK: {DateTime.Now:HH:mm:ss}";

        if (result is not null)
        {
            LoadRecords(result.Records);
            PublishInfo(true, $"Auto ciclo -> procesados={result.Processed}, dup={result.Duplicates}");
        }
    }

    private void OnLogPublished(ProcessingLogEntry entry)
    {
        var message = $"[{entry.Timestamp:HH:mm:ss}] [{entry.Tag}] {entry.Message}";
        if (entry.Automatic)
        {
            AutomaticoLogs.Add(message);
            return;
        }

        ManualLogs.Add(message);
    }

    private void PublishInfo(bool automatic, string message)
    {
        OnLogPublished(new ProcessingLogEntry(DateTimeOffset.Now, automatic ? "AUTO" : "INFO", message, automatic));
    }

    private void LoadRecords(IEnumerable<ProcessedEmailRecord> records)
    {
        Registros.Clear();
        foreach (var record in records.OrderByDescending(r => r.Consecutivo))
        {
            Registros.Add(record);
        }
    }
}
