namespace videoCut.Services;

public class VideoExportResult
{
    private VideoExportResult(bool isSuccess, string outputPath, string errorMessage)
    {
        IsSuccess = isSuccess;
        OutputPath = outputPath;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string OutputPath { get; }

    public string ErrorMessage { get; }

    public static VideoExportResult Success(string outputPath) => new(true, outputPath, string.Empty);

    public static VideoExportResult Failure(string errorMessage) => new(false, string.Empty, errorMessage);
}
