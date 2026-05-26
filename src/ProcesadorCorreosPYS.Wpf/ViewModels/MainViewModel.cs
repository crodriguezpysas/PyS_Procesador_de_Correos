using System.Collections.ObjectModel;
using System.Windows.Input;
using ProcesadorCorreosPYS.Wpf.Commands;

namespace ProcesadorCorreosPYS.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private DateTime _fechaObjetivo = DateTime.Today;
    private string _rangoSubcarpetas = "1-5";

    public MainViewModel()
    {
        ProcesarAhoraCommand = new RelayCommand(() => ManualLogs.Add($"[{DateTime.Now:HH:mm:ss}] Inicio procesamiento manual ({FechaObjetivo:yyyy-MM-dd})"));
        LimpiarCommand = new RelayCommand(() => ManualLogs.Clear());
        GenerarStickersCommand = new RelayCommand(() => ManualLogs.Add($"[{DateTime.Now:HH:mm:ss}] Sticker manual para rango/lista: {RangoSubcarpetas}"));
        CrearExcelCommand = new RelayCommand(() => AutomaticoLogs.Add($"[{DateTime.Now:HH:mm:ss}] Excel incremental actualizado"));

        ManualLogs.Add("[INFO] Sistema listo en modo manual.");
        AutomaticoLogs.Add("[WAIT] Modo automático en espera de horario configurado.");
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

    public ObservableCollection<string> ManualLogs { get; } = [];
    public ObservableCollection<string> AutomaticoLogs { get; } = [];

    public ICommand ProcesarAhoraCommand { get; }
    public ICommand LimpiarCommand { get; }
    public ICommand GenerarStickersCommand { get; }
    public ICommand CrearExcelCommand { get; }
}
