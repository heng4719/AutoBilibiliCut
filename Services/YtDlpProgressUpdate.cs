namespace videoCut.Services;

public class YtDlpProgressUpdate
{
    public string Title { get; init; } = string.Empty;

    public string StatusMessage { get; init; } = string.Empty;

    public double? Percent { get; init; }

    public string SpeedText { get; init; } = string.Empty;

    public string EtaText { get; init; } = string.Empty;
}
