using System.Diagnostics;
using System.Globalization;

namespace videoCut.Services;

public class VideoMetadataService
{
    public async Task<VideoMetadataResult> ReadAsync(string filePath)
    {
        var ffprobePath = FfmpegPathResolver.Resolve("ffprobe");
        if (ffprobePath is null)
        {
            return VideoMetadataResult.Failure("未找到 ffprobe，暂时无法读取视频时长。请先安装 FFmpeg，或把 ffprobe.exe 放到程序目录。");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ffprobePath,
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
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
            var message = string.IsNullOrWhiteSpace(error) ? "ffprobe 执行失败。" : error;
            return VideoMetadataResult.Failure($"读取视频时长失败：{message}");
        }

        if (!double.TryParse(output, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            return VideoMetadataResult.Failure("ffprobe 返回的时长格式无法识别。");
        }

        return VideoMetadataResult.Success(FormatDuration(seconds));
    }
    private static string FormatDuration(double totalSeconds)
    {
        var duration = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return duration.ToString(@"hh\:mm\:ss");
    }
}
