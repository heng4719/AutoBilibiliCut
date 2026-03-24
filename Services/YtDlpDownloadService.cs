using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace videoCut.Services;

public class YtDlpDownloadService
{
    public async Task<YtDlpDownloadResult> DownloadVideoAsync(
        string videoUrl,
        string? customFileName,
        IProgress<YtDlpProgressUpdate>? progress = null)
    {
        var projectRoot = ResolveProjectRoot();
        var ytDlpPath = Path.Combine(projectRoot, "tools", "yt-dlp", "yt-dlp.exe");
        if (!File.Exists(ytDlpPath))
        {
            return YtDlpDownloadResult.Failure("未找到 yt-dlp.exe，请先确认 tools/yt-dlp/yt-dlp.exe 存在。");
        }

        var ffmpegDirectory = Path.Combine(projectRoot, "tools", "ffmpeg", "bin");
        if (!Directory.Exists(ffmpegDirectory))
        {
            return YtDlpDownloadResult.Failure("未找到 FFmpeg 目录，请先确认 tools/ffmpeg/bin 存在。");
        }

        var downloadDirectory = Path.Combine(projectRoot, "download");
        Directory.CreateDirectory(downloadDirectory);

        var outputTemplate = BuildOutputTemplate(customFileName);
        var startInfo = CreateStartInfo(ytDlpPath, projectRoot, downloadDirectory, ffmpegDirectory, outputTemplate, videoUrl);

        using var process = new Process { StartInfo = startInfo };
        var errorLines = new List<string>();
        var outputFilePath = string.Empty;

        process.Start();

        var outputTask = ReadLinesAsync(process.StandardOutput, line =>
        {
            if (TryParseTitleLine(line, out var title))
            {
                progress?.Report(new YtDlpProgressUpdate
                {
                    Title = title,
                    StatusMessage = $"正在下载：{title}"
                });
                return;
            }

            if (TryParseFilePathLine(line, out var filePath))
            {
                outputFilePath = filePath;
                progress?.Report(new YtDlpProgressUpdate
                {
                    StatusMessage = "正在整理下载文件..."
                });
                return;
            }

            if (TryParseProgressLine(line, out var percent, out var speed, out var eta))
            {
                progress?.Report(new YtDlpProgressUpdate
                {
                    Percent = percent,
                    SpeedText = speed,
                    EtaText = eta,
                    StatusMessage = "下载中"
                });
                return;
            }

            if (line.Contains("Merging formats", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report(new YtDlpProgressUpdate
                {
                    StatusMessage = "正在合并音视频..."
                });
            }
        });

        var errorTask = ReadLinesAsync(process.StandardError, line =>
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                errorLines.Add(line.Trim());
            }
        });

        await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask);

        if (process.ExitCode != 0)
        {
            var message = errorLines.LastOrDefault();
            return YtDlpDownloadResult.Failure(string.IsNullOrWhiteSpace(message) ? "yt-dlp 下载失败。" : message);
        }

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            outputFilePath = FindLatestVideoFile(downloadDirectory);
        }

        return YtDlpDownloadResult.Success(downloadDirectory, outputFilePath);
    }

    private static ProcessStartInfo CreateStartInfo(
        string ytDlpPath,
        string projectRoot,
        string downloadDirectory,
        string ffmpegDirectory,
        string outputTemplate,
        string videoUrl)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = projectRoot
        };

        startInfo.ArgumentList.Add("-P");
        startInfo.ArgumentList.Add(downloadDirectory);
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(outputTemplate);
        startInfo.ArgumentList.Add("--merge-output-format");
        startInfo.ArgumentList.Add("mp4");
        startInfo.ArgumentList.Add("--ffmpeg-location");
        startInfo.ArgumentList.Add(ffmpegDirectory);
        startInfo.ArgumentList.Add("--no-playlist");
        startInfo.ArgumentList.Add("--newline");
        startInfo.ArgumentList.Add("--progress");
        startInfo.ArgumentList.Add("--no-colors");
        startInfo.ArgumentList.Add("--progress-template");
        startInfo.ArgumentList.Add("download:%(progress._percent_str)s|%(progress._speed_str)s|%(progress._eta_str)s");
        startInfo.ArgumentList.Add("--print");
        startInfo.ArgumentList.Add("before_dl:title:%(title)s");
        startInfo.ArgumentList.Add("--print");
        startInfo.ArgumentList.Add("after_move:filepath:%(filepath)s");
        startInfo.ArgumentList.Add(videoUrl);

        return startInfo;
    }

    private static async Task ReadLinesAsync(StreamReader reader, Action<string> handleLine)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                handleLine(line.Trim());
            }
        }
    }

    private static bool TryParseTitleLine(string line, out string title)
    {
        const string prefix = "title:";
        if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            title = line[prefix.Length..].Trim();
            return true;
        }

        title = string.Empty;
        return false;
    }

    private static bool TryParseFilePathLine(string line, out string filePath)
    {
        const string prefix = "filepath:";
        if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            filePath = line[prefix.Length..].Trim();
            return true;
        }

        filePath = string.Empty;
        return false;
    }

    private static bool TryParseProgressLine(string line, out double? percent, out string speed, out string eta)
    {
        const string prefix = "download:";
        if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            percent = null;
            speed = string.Empty;
            eta = string.Empty;
            return false;
        }

        var parts = line[prefix.Length..].Split('|');
        var percentText = parts.ElementAtOrDefault(0)?.Replace("%", string.Empty).Trim() ?? string.Empty;
        speed = parts.ElementAtOrDefault(1)?.Trim() ?? string.Empty;
        eta = parts.ElementAtOrDefault(2)?.Trim() ?? string.Empty;

        if (double.TryParse(percentText, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            percent = Math.Clamp(value, 0, 100);
            return true;
        }

        percent = null;
        return true;
    }

    private static string BuildOutputTemplate(string? customFileName)
    {
        if (string.IsNullOrWhiteSpace(customFileName))
        {
            return "%(title)s.%(ext)s";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(customFileName.Trim().Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "%(title)s.%(ext)s";
        }

        return $"{sanitized}.%(ext)s";
    }

    private static string FindLatestVideoFile(string downloadDirectory)
    {
        return new DirectoryInfo(downloadDirectory)
            .GetFiles()
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => file.FullName)
            .FirstOrDefault() ?? string.Empty;
    }

    private static string ResolveProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var csprojPath = Path.Combine(current.FullName, "videoCut.csproj");
            if (File.Exists(csprojPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
