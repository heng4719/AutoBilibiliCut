namespace videoCut.Models;

public class DownloadedVideoFile
{
    public string Name { get; init; } = string.Empty;

    public string FileSize { get; init; } = "--";

    public string ModifiedTime { get; init; } = "--";

    public string FullPath { get; init; } = string.Empty;
}
