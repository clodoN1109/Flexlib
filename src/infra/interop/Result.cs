namespace Flexlib.Infrastructure.Interop;

public class Result
{
    public string? SuccessMessage { get; }
    public string? WarningMessage { get; }
    public string? ErrorMessage { get; }
    public object? Payload { get; }

    public bool IsSuccess { get; }
    public bool IsWarning { get; }
    public bool IsFailure => !IsSuccess;
    public bool IsFailureOrWarning => IsFailure || IsWarning;
    public string Message => string.Join("\n\n", new List<string> 
    {
        SuccessMessage ?? "",
        WarningMessage ?? "",
        ErrorMessage ?? ""
    });

    private Result(
        bool isSuccess,
        bool isWarning,
        string? successMessage,
        string? errorMessage,
        string? warningMessage,
        object? payload = null)
    {
        IsSuccess = isSuccess;
        IsWarning = isWarning;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
        WarningMessage = warningMessage;
        Payload = payload; // ← This was missing
    }

    public static Result Success(string message, object? payload = null) =>
        new(true, false, message, null, null, payload);

    public static Result Warn(string warning, string success = "", object? payload = null) =>
        new(false, true, success, null, warning, payload);

    public static Result Fail(string message, object? payload = null) =>
        new(false, false, null, message, null, payload);
}