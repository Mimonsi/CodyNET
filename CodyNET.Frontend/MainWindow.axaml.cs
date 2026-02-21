using System.Globalization;
using Avalonia.Controls;
using CodyNET.Frontend.Controls;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private ScreenControl? screen;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
    }
    
    private void InitializeScreen()
    {
        screen = this.FindControl<ScreenControl>("Screen");
        if (screen != null)
        {
            screen.ScaleFactor = 6.0;
        }
    }

    private void OnScaleMenuClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (screen == null || sender is not MenuItem menuItem)
        {
            return;
        }

        if (menuItem.Tag is string tagValue
            && double.TryParse(tagValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            screen.ScaleFactor = scale;
        }
    }

}
