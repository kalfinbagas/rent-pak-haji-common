namespace RentPakHaji.Common.Application;

/// <summary>
/// Discriminated union result — either Success or Failure.
/// Avoids throwing exceptions for business-rule failures.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? errorCode = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static Result Success() => new(true);
    public static Result Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);

    public static Result<TValue> Success<TValue>(TValue value) =>
        Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(string errorCode, string errorMessage) =>
        Result<TValue>.Failure(errorCode, errorMessage);
}

/// <summary>Result with a value payload on success.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base(true)
    {
        _value = value;
    }

    private Result(string errorCode, string errorMessage)
        : base(false, errorCode, errorMessage) { }

    public TValue Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(string errorCode, string errorMessage) =>
        new(errorCode, errorMessage);
}
