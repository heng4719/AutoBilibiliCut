using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using videoCut.Services;
using videoCut.ViewModels;

namespace videoCut;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            new VideoMetadataService(),
            new VideoExportService(),
            new YtDlpDownloadService());
        RegisterNumericInputHandlers();
    }

    private void RegisterNumericInputHandlers()
    {
        var comboBoxes = new[]
        {
            StartHourComboBox,
            StartMinuteComboBox,
            StartSecondComboBox,
            EndHourComboBox,
            EndMinuteComboBox,
            EndSecondComboBox
        };

        foreach (var comboBox in comboBoxes)
        {
            System.Windows.DataObject.AddPastingHandler(comboBox, NumericComboBox_OnPaste);
        }
    }

    private void NumericComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(ch => !char.IsDigit(ch));
    }

    private void NumericComboBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var allowedKeys = new[]
        {
            Key.Back, Key.Delete, Key.Tab, Key.Left, Key.Right,
            Key.Home, Key.End, Key.Enter
        };

        if (allowedKeys.Contains(e.Key))
        {
            return;
        }

        var isDigitKey = (e.Key >= Key.D0 && e.Key <= Key.D9)
            || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

        e.Handled = !isDigitKey;
    }

    private void NumericComboBox_OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(System.Windows.DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        var text = e.DataObject.GetData(System.Windows.DataFormats.Text) as string ?? string.Empty;
        if (text.Any(ch => !char.IsDigit(ch)))
        {
            e.CancelCommand();
        }
    }
}
