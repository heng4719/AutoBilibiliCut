using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace videoCut.Models;

public class SegmentDraft : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _startTime = string.Empty;
    private string _endTime = string.Empty;
    private string _duration = "--";
    private string _status = "待填写";
    private string _exportedFilePath = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public string EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public string Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string ExportedFilePath
    {
        get => _exportedFilePath;
        set => SetProperty(ref _exportedFilePath, value);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
