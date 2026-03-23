using System.IO;

namespace videoCut.Services;

public static class FfmpegPathResolver
{
    public static string? Resolve(string executableName)
    {
        var preferredCandidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "tools", "ffmpeg", "bin", $"{executableName}.exe"),
            Path.Combine(AppContext.BaseDirectory, $"{executableName}.exe")
        };

        foreach (var candidate in preferredCandidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
