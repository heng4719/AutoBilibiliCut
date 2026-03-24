namespace videoCut.Services;

public class YtDlpDownloadResult
{
    private YtDlpDownloadResult(bool isSuccess, string downloadDirectory, string outputFilePath, string errorMessage)
    {
        IsSuccess = isSuccess;
        DownloadDirectory = downloadDirectory;
        OutputFilePath = outputFilePath;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string DownloadDirectory { get; }

    public string OutputFilePath { get; }

    public string ErrorMessage { get; }

    public static YtDlpDownloadResult Success(string downloadDirectory, string outputFilePath) =>
        new(true, downloadDirectory, outputFilePath, string.Empty);

    public static YtDlpDownloadResult Failure(string errorMessage) =>
        new(false, string.Empty, string.Empty, errorMessage);
}
