using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using videoCut.Models;
using videoCut.Services;

namespace videoCut.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private const string TestDownloadUrl = "https://www.bilibili.com/video/BV1mfAHzAEZg";

    private readonly VideoMetadataService _videoMetadataService;
    private readonly VideoExportService _videoExportService;
    private readonly YtDlpDownloadService _ytDlpDownloadService;

    private string _videoName = "尚未选择视频";
    private string _videoSize = "--";
    private string _videoDuration = "--:--:--";
    private string _videoPath = "请选择一个本地视频文件";
    private string _videoStatus = "请选择一个本地视频文件，程序会读取名称、大小、路径和视频时长。";
    private string _outputDirectory = "尚未选择输出目录";
    private string _exportStatus = "等待配置切片任务";
    private bool _isExportStatusError;
    private string _newSegmentName = string.Empty;
    private string _newSegmentStartHour = "00";
    private string _newSegmentStartMinute = "00";
    private string _newSegmentStartSecond = "00";
    private string _newSegmentEndHour = "00";
    private string _newSegmentEndMinute = "00";
    private string _newSegmentEndSecond = "00";
    private TimeSpan? _videoDurationValue;
    private bool _isExporting;
    private bool _isTestingDownload;

    public MainWindowViewModel(
        VideoMetadataService videoMetadataService,
        VideoExportService videoExportService,
        YtDlpDownloadService ytDlpDownloadService)
    {
        _videoMetadataService = videoMetadataService;
        _videoExportService = videoExportService;
        _ytDlpDownloadService = ytDlpDownloadService;

        SelectVideoCommand = new RelayCommand(async _ => await SelectVideoAsync());
        ClearVideoCommand = new RelayCommand(_ => ClearVideo());
        AddSegmentCommand = new RelayCommand(_ => AddSegment());
        ClearSegmentsCommand = new RelayCommand(_ => ClearSegments());
        RemoveSegmentCommand = new RelayCommand(segment => RemoveSegment(segment as SegmentDraft));
        SelectOutputDirectoryCommand = new RelayCommand(_ => SelectOutputDirectory());
        ExportSegmentsCommand = new RelayCommand(async _ => await ExportSegmentsAsync());
        TestDownloadCommand = new RelayCommand(async _ => await TestDownloadAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand SelectVideoCommand { get; }

    public ICommand ClearVideoCommand { get; }

    public ICommand AddSegmentCommand { get; }

    public ICommand ClearSegmentsCommand { get; }

    public ICommand RemoveSegmentCommand { get; }

    public ICommand SelectOutputDirectoryCommand { get; }

    public ICommand ExportSegmentsCommand { get; }

    public ICommand TestDownloadCommand { get; }

    public string VideoName
    {
        get => _videoName;
        private set => SetProperty(ref _videoName, value);
    }

    public string VideoSize
    {
        get => _videoSize;
        private set => SetProperty(ref _videoSize, value);
    }

    public string VideoDuration
    {
        get => _videoDuration;
        private set => SetProperty(ref _videoDuration, value);
    }

    public string VideoPath
    {
        get => _videoPath;
        private set => SetProperty(ref _videoPath, value);
    }

    public string VideoStatus
    {
        get => _videoStatus;
        private set => SetProperty(ref _videoStatus, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        private set => SetProperty(ref _outputDirectory, value);
    }

    public string ExportStatus
    {
        get => _exportStatus;
        private set => SetProperty(ref _exportStatus, value);
    }

    public bool IsExportStatusError
    {
        get => _isExportStatusError;
        private set => SetProperty(ref _isExportStatusError, value);
    }

    public string NewSegmentName
    {
        get => _newSegmentName;
        set => SetProperty(ref _newSegmentName, value);
    }

    public string NewSegmentStartHour
    {
        get => _newSegmentStartHour;
        set => SetProperty(ref _newSegmentStartHour, value);
    }

    public string NewSegmentStartMinute
    {
        get => _newSegmentStartMinute;
        set => SetProperty(ref _newSegmentStartMinute, value);
    }

    public string NewSegmentStartSecond
    {
        get => _newSegmentStartSecond;
        set => SetProperty(ref _newSegmentStartSecond, value);
    }

    public string NewSegmentEndHour
    {
        get => _newSegmentEndHour;
        set => SetProperty(ref _newSegmentEndHour, value);
    }

    public string NewSegmentEndMinute
    {
        get => _newSegmentEndMinute;
        set => SetProperty(ref _newSegmentEndMinute, value);
    }

    public string NewSegmentEndSecond
    {
        get => _newSegmentEndSecond;
        set => SetProperty(ref _newSegmentEndSecond, value);
    }

    public ObservableCollection<SegmentDraft> Segments { get; } =
    [];

    public IReadOnlyList<string> HourOptions { get; } = BuildOptions(100);

    public IReadOnlyList<string> MinuteSecondOptions { get; } = BuildOptions(60);

    public bool HasSelectedVideo => !string.IsNullOrWhiteSpace(VideoPath) && File.Exists(VideoPath);

    public void RemoveSegment(SegmentDraft? segment)
    {
        if (segment is null)
        {
            return;
        }

        if (_isExporting)
        {
            return;
        }

        UnsubscribeSegment(segment);
        Segments.Remove(segment);
        UpdateSegmentSummary();
    }

    private async Task SelectVideoAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择视频文件",
            Filter = "视频文件|*.mp4;*.mov;*.avi;*.mkv;*.wmv;*.flv;*.m4v|所有文件|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await LoadVideoAsync(dialog.FileName);
    }

    private async Task LoadVideoAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        VideoName = fileInfo.Name;
        VideoSize = FormatFileSize(fileInfo.Length);
        VideoPath = fileInfo.FullName;
        VideoDuration = "读取中...";
        VideoStatus = "正在读取视频时长...";
        SetExportStatus("已选择视频，等待配置切片任务");

        var result = await _videoMetadataService.ReadAsync(filePath);
        if (result.IsSuccess)
        {
            VideoDuration = result.DurationText;
            _videoDurationValue = result.Duration;
            VideoStatus = "视频信息读取完成。";
            RevalidateSegments();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedVideo)));
            return;
        }

        VideoDuration = "无法读取";
        _videoDurationValue = null;
        VideoStatus = result.ErrorMessage;
        RevalidateSegments();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedVideo)));
    }

    private void ClearVideo()
    {
        VideoName = "尚未选择视频";
        VideoSize = "--";
        VideoDuration = "--:--:--";
        VideoPath = "请选择一个本地视频文件";
        VideoStatus = "请选择一个本地视频文件，程序会读取名称、大小、路径和视频时长。";
        SetExportStatus("等待配置切片任务");
        _videoDurationValue = null;
        ClearSegments();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedVideo)));
    }

    private void AddSegment()
    {
        if (_isExporting)
        {
            return;
        }

        if (!HasSelectedVideo)
        {
            SetExportStatus("请先选择视频文件，再新增切片", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewSegmentName)
            && IsDefaultTime(NewSegmentStartHour, NewSegmentStartMinute, NewSegmentStartSecond)
            && IsDefaultTime(NewSegmentEndHour, NewSegmentEndMinute, NewSegmentEndSecond))
        {
            SetExportStatus("请先填写切片名称或时间范围", true);
            return;
        }

        var segment = new SegmentDraft
        {
            Name = string.IsNullOrWhiteSpace(NewSegmentName) ? $"切片 {Segments.Count + 1}" : NewSegmentName.Trim(),
            StartTime = ComposeTime(NewSegmentStartHour, NewSegmentStartMinute, NewSegmentStartSecond),
            EndTime = ComposeTime(NewSegmentEndHour, NewSegmentEndMinute, NewSegmentEndSecond),
            Duration = "--",
            Status = "待填写"
        };

        segment.PropertyChanged += SegmentOnPropertyChanged;
        Segments.Add(segment);
        ValidateSegment(segment);
        NewSegmentName = string.Empty;
        ResetNewSegmentTime();
        UpdateSegmentSummary();
    }

    private void ClearSegments()
    {
        if (_isExporting)
        {
            return;
        }

        foreach (var segment in Segments)
        {
            UnsubscribeSegment(segment);
        }

        Segments.Clear();
        UpdateSegmentSummary();
    }

    private void SegmentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isExporting)
        {
            return;
        }

        if (sender is not SegmentDraft segment)
        {
            return;
        }

        if (e.PropertyName is nameof(SegmentDraft.Name) or nameof(SegmentDraft.StartTime) or nameof(SegmentDraft.EndTime))
        {
            ValidateSegment(segment);
            UpdateSegmentSummary();
        }
    }

    private void ValidateSegment(SegmentDraft segment)
    {
        if (string.IsNullOrWhiteSpace(segment.Name))
        {
            segment.Duration = "--";
            segment.Status = "名称不能为空";
            return;
        }

        if (!TryParseTime(segment.StartTime, out var start))
        {
            segment.Duration = "--";
            segment.Status = "开始时间格式错误";
            return;
        }

        if (!TryParseTime(segment.EndTime, out var end))
        {
            segment.Duration = "--";
            segment.Status = "结束时间格式错误";
            return;
        }

        if (end <= start)
        {
            segment.Duration = "--";
            segment.Status = "结束时间必须大于开始时间";
            return;
        }

        if (_videoDurationValue.HasValue && end > _videoDurationValue.Value)
        {
            segment.Duration = "--";
            segment.Status = "结束时间超过视频总时长";
            return;
        }

        segment.Duration = (end - start).ToString(@"hh\:mm\:ss");
        segment.Status = "可导出";
    }

    private void SelectOutputDirectory()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "选择切片输出目录",
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrWhiteSpace(OutputDirectory) && Directory.Exists(OutputDirectory))
        {
            dialog.SelectedPath = OutputDirectory;
        }

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            return;
        }

        OutputDirectory = dialog.SelectedPath;
        SetExportStatus("输出目录已更新");
        UpdateSegmentSummary();
    }

    private async Task ExportSegmentsAsync()
    {
        if (_isExporting || _isTestingDownload)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(VideoPath) || !File.Exists(VideoPath))
        {
            SetExportStatus("请先选择视频文件", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
        {
            SetExportStatus("请先选择有效的输出目录", true);
            return;
        }

        if (Segments.Count == 0)
        {
            SetExportStatus("没有可导出的切片，请先修正切片配置", true);
            return;
        }

        RevalidateSegments();

        var invalidSegments = Segments.Where(x => x.Status != "可导出").ToList();
        if (invalidSegments.Count > 0)
        {
            SetExportStatus("存在异常状态的切片，请先修正后再导出", true);
            return;
        }

        var exportableSegments = Segments.ToList();

        _isExporting = true;
        var successCount = 0;
        var failureCount = 0;

        foreach (var segment in exportableSegments)
        {
            segment.Status = "导出中";
            SetExportStatus($"正在导出 {segment.Name}...");

            var result = await _videoExportService.ExportAsync(VideoPath, OutputDirectory, segment);
            if (result.IsSuccess)
            {
                segment.Status = "导出成功";
                successCount++;
            }
            else
            {
                segment.Status = $"导出失败";
                failureCount++;
                VideoStatus = result.ErrorMessage;
                SetExportStatus($"导出失败：{segment.Name}", true);
            }
        }

        _isExporting = false;
        SetExportStatus($"导出完成，成功 {successCount} 个，失败 {failureCount} 个", failureCount > 0);
    }

    private async Task TestDownloadAsync()
    {
        if (_isExporting || _isTestingDownload)
        {
            return;
        }

        _isTestingDownload = true;
        SetExportStatus("正在执行测试下载...");

        var result = await _ytDlpDownloadService.DownloadTestVideoAsync(TestDownloadUrl);
        if (result.IsSuccess)
        {
            SetExportStatus($"测试下载完成，文件已保存到 {result.DownloadDirectory}");
        }
        else
        {
            SetExportStatus($"测试下载失败：{result.ErrorMessage}", true);
        }

        _isTestingDownload = false;
    }

    private void RevalidateSegments()
    {
        foreach (var segment in Segments)
        {
            ValidateSegment(segment);
        }

        UpdateSegmentSummary();
    }

    private void UpdateSegmentSummary()
    {
        if (_isExporting)
        {
            return;
        }

        if (Segments.Count == 0)
        {
            SetExportStatus("请先添加切片任务");
            return;
        }

        var readyCount = Segments.Count(x => x.Status == "可导出");
        var invalidCount = Segments.Count - readyCount;
        SetExportStatus($"当前共 {Segments.Count} 个切片，其中 {readyCount} 个可导出，{invalidCount} 个待修正", invalidCount > 0);
    }

    private static bool TryParseTime(string? input, out TimeSpan value)
    {
        var formats = new[] { @"h\:m\:s", @"hh\:mm\:ss", @"m\:s", @"mm\:ss" };
        foreach (var format in formats)
        {
            if (TimeSpan.TryParseExact(input, format, null, out value))
            {
                return true;
            }
        }

        value = TimeSpan.Zero;
        return false;
    }

    private static bool IsDefaultTime(string hour, string minute, string second)
    {
        return NormalizeTimePart(hour, 2) == "00"
            && NormalizeTimePart(minute, 2) == "00"
            && NormalizeTimePart(second, 2) == "00";
    }

    private static string ComposeTime(string hour, string minute, string second)
    {
        return $"{NormalizeTimePart(hour, 2)}:{NormalizeTimePart(minute, 2)}:{NormalizeTimePart(second, 2)}";
    }

    private static string NormalizeTimePart(string? input, int width)
    {
        var text = string.IsNullOrWhiteSpace(input) ? "0" : input.Trim();
        if (int.TryParse(text, out var value) && value >= 0)
        {
            return value.ToString($"D{width}");
        }

        return text;
    }

    private void ResetNewSegmentTime()
    {
        NewSegmentStartHour = "00";
        NewSegmentStartMinute = "00";
        NewSegmentStartSecond = "00";
        NewSegmentEndHour = "00";
        NewSegmentEndMinute = "00";
        NewSegmentEndSecond = "00";
    }

    private static IReadOnlyList<string> BuildOptions(int count)
    {
        return Enumerable.Range(0, count)
            .Select(x => x.ToString("D2"))
            .ToArray();
    }

    private void SetExportStatus(string message, bool isError = false)
    {
        ExportStatus = message;
        IsExportStatusError = isError;
    }

    private void UnsubscribeSegment(SegmentDraft segment)
    {
        segment.PropertyChanged -= SegmentOnPropertyChanged;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
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
