using System.Diagnostics;
using System.IO;
using videoCut.Models;

namespace videoCut.Services;

public class VideoExportService
{
    public async Task<VideoExportResult> ExportAsync(string inputPath, string outputDirectory, SegmentDraft segment)
    {
        var ffmpegPath = FfmpegPathResolver.Resolve("ffmpeg");
        if (ffmpegPath is null)
        {
            return VideoExportResult.Failure("未找到 ffmpeg，暂时无法导出切片。请先安装 FFmpeg，或把 ffmpeg.exe 放到程序目录。");
        }

        Directory.CreateDirectory(outputDirectory);
        var outputPath = BuildOutputPath(outputDirectory, segment.Name);
        var arguments =
            $"-y -ss {segment.StartTime} -to {segment.EndTime} -i \"{inputPath}\" -c copy \"{outputPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
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
            var message = string.IsNullOrWhiteSpace(error) ? output : error;
            return VideoExportResult.Failure(string.IsNullOrWhiteSpace(message) ? "ffmpeg 执行失败。" : message);
        }

        return VideoExportResult.Success(outputPath);
    }

    private static string BuildOutputPath(string outputDirectory, string segmentName)
    {
        var safeFileName = string.Join("_", segmentName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "切片";
        }

        var fileName = $"{safeFileName}.mp4";
        var fullPath = Path.Combine(outputDirectory, fileName);
        var index = 1;

        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(outputDirectory, $"{safeFileName}_{index}.mp4");
            index++;
        }

        return fullPath;
    }
}
