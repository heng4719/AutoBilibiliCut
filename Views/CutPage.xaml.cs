using System.Linq;
using System.Windows;

namespace videoCut.Views;

public partial class CutPage : System.Windows.Controls.UserControl
{
    public CutPage()
    {
        InitializeComponent();
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

    private void NumericComboBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(ch => !char.IsDigit(ch));
    }

    private void NumericComboBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var allowedKeys = new[]
        {
            System.Windows.Input.Key.Back, System.Windows.Input.Key.Delete, System.Windows.Input.Key.Tab, System.Windows.Input.Key.Left, System.Windows.Input.Key.Right,
            System.Windows.Input.Key.Home, System.Windows.Input.Key.End, System.Windows.Input.Key.Enter
        };

        if (allowedKeys.Contains(e.Key))
        {
            return;
        }

        var isDigitKey = (e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.D9)
            || (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9);

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
