namespace videoCut.Services;

public class VideoMetadataResult
{
    private VideoMetadataResult(bool isSuccess, string durationText, TimeSpan? duration, string errorMessage)
    {
        IsSuccess = isSuccess;
        DurationText = durationText;
        Duration = duration;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string DurationText { get; }

    public TimeSpan? Duration { get; }

    public string ErrorMessage { get; }

    public static VideoMetadataResult Success(string durationText) =>
        new(true, durationText, ParseDuration(durationText), string.Empty);

    public static VideoMetadataResult Failure(string errorMessage) =>
        new(false, string.Empty, null, errorMessage);

    private static TimeSpan? ParseDuration(string durationText)
    {
        return TimeSpan.TryParse(durationText, out var value) ? value : null;
    }
}
