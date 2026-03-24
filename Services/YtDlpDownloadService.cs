using System.Diagnostics;
using System.IO;

namespace videoCut.Services;

public class YtDlpDownloadService
{
    public async Task<YtDlpDownloadResult> DownloadTestVideoAsync(string videoUrl)
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

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = $"-P \"{downloadDirectory}\" --merge-output-format mp4 --ffmpeg-location \"{ffmpegDirectory}\" --no-playlist \"{videoUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = projectRoot
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = (await outputTask).Trim();
        var error = (await errorTask).Trim();

        if (process.ExitCode != 0)
        {
            var message = string.IsNullOrWhiteSpace(error) ? output : error;
            return YtDlpDownloadResult.Failure(string.IsNullOrWhiteSpace(message) ? "yt-dlp 下载失败。" : message);
        }

        return YtDlpDownloadResult.Success(downloadDirectory);
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
