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
    }
}
