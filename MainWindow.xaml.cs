using System.Windows;
using System.Windows.Input;
using FontAwesome.WPF;
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
        UpdateWindowStateButton();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void ToggleWindowStateButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        UpdateWindowStateButton();
    }

    private void UpdateWindowStateButton()
    {
        if (ToggleWindowStateButton is null)
        {
            return;
        }

        var icon = new ImageAwesome
        {
            Icon = WindowState == WindowState.Maximized ? FontAwesomeIcon.WindowRestore : FontAwesomeIcon.WindowMaximize,
            Width = 14,
            Height = 14
        };

        icon.SetBinding(ImageAwesome.ForegroundProperty, new System.Windows.Data.Binding("Foreground")
        {
            Source = ToggleWindowStateButton
        });

        ToggleWindowStateButton.Content = icon;
        ToggleWindowStateButton.ToolTip = WindowState == WindowState.Maximized ? "还原" : "最大化";
    }
}
