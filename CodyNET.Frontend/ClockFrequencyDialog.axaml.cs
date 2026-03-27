using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CodyNET.Common.Utils;

namespace CodyNET.Frontend;

public partial class ClockFrequencyDialog : Window
{
    public long? ResultHz { get; private set; }

    public ClockFrequencyDialog()
    {
        InitializeComponent();
        Opened += (_, _) => FrequencyInput.Focus();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TryApply();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e) => TryApply();
    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close();

    private void TryApply()
    {
        if (Unit.TryParseSi(FrequencyInput.Text, "Hz", out var hz))
        {
            ResultHz = hz;
            Close();
        }
        else
        {
            ErrorLabel.Text = "Invalid format. Try: 3.5 MHz, 500 kHz or 3500000";
            ErrorLabel.IsVisible = true;
        }
    }
}
