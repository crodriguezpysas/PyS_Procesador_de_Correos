using System.Windows;

namespace ProcesadorCorreosPYS.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnSalirClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
