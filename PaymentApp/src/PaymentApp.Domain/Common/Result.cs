namespace PaymentApp.Domain.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Use for expected failures (validation). Use exceptions for exceptional failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Result with a value on success.
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}