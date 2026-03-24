namespace videoCut.Services;

public class YtDlpDownloadResult
{
    private YtDlpDownloadResult(bool isSuccess, string downloadDirectory, string errorMessage)
    {
        IsSuccess = isSuccess;
        DownloadDirectory = downloadDirectory;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string DownloadDirectory { get; }

    public string ErrorMessage { get; }

    public static YtDlpDownloadResult Success(string downloadDirectory) =>
        new(true, downloadDirectory, string.Empty);

    public static YtDlpDownloadResult Failure(string errorMessage) =>
        new(false, string.Empty, errorMessage);
}
