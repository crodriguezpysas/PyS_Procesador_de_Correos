using System.Windows;
using ProcesadorCorreosPYS.Application.Abstractions;
using ProcesadorCorreosPYS.Domain.Models;

namespace ProcesadorCorreosPYS.Wpf.Logging;

public sealed class ObservableLogSink : ILogSink
{
    public event Action<ProcessingLogEntry>? EntryPublished;

    public void Publish(ProcessingLogEntry entry)
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher)
        {
            _ = dispatcher.InvokeAsync(() => EntryPublished?.Invoke(entry));
        }
        else
        {
            EntryPublished?.Invoke(entry);
        }
    }
}
