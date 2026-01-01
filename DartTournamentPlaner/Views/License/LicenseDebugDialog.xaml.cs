using System.Windows;
using System.Windows.Input;

namespace DartTournamentPlaner.Views.License;

public partial class LicenseDebugDialog : Window
{
    public LicenseDebugDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => this.Focus();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
