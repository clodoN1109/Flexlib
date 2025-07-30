namespace Flexlib.Infrastructure.Interop;

public class Result
{
    public bool IsSuccess { get; }
    public string? SuccessMessage { get; }
    public string? ErrorMessage { get; }

    public bool IsFailure => IsSuccess == false;

    private Result(bool isSuccess, string? successMessage, string? errorMessage)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
    }

    public static Result Success(string message) => new(true, message, null);
    public static Result Fail(string message) => new(false, null, message);
}

