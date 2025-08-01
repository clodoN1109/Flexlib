namespace Flexlib.Infrastructure.Interop;

public class Result
{
    public string? SuccessMessage { get; }
    public string? WarningMessage { get; }
    public string? ErrorMessage { get; }

    public bool IsSuccess { get; }
    public bool IsWarning { get; }
    public bool IsFailure => !IsSuccess;

    public string Message => SuccessMessage ?? WarningMessage ?? ErrorMessage ?? "";

    private Result(bool isSuccess, bool isWarning, string? successMessage, string? errorMessage, string? warningMessage)
    {
        IsSuccess = isSuccess;
        IsWarning = isWarning;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
        WarningMessage = warningMessage;
    }

    public static Result Success(string message) => new(true, false, message, null, null);
    public static Result Warn(string message) => new(false, true, null, null, message);
    public static Result Fail(string message) => new(false, false, null, message, null);
}
