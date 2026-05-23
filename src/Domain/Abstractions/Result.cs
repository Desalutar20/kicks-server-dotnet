using System.Runtime.CompilerServices;

namespace Domain.Abstractions;

public record Result
{
    protected Result(bool isSuccess, Error? error, string? sourceMember = null, int sourceLine = 0)
    {
        if ((isSuccess && error is not null) || (!isSuccess && error is null))
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;

        SourceMember = sourceMember;
        SourceLine = sourceLine;
    }

    protected string? SourceMember { get; }
    protected int SourceLine { get; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error =>
        IsFailure
            ? field!
            : throw new InvalidOperationException("Cannot access error of success result.");

    public static Result Success() => new(true, null);

    public static Result Failure(
        Error error,
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => new(false, error, member, line);

    public static implicit operator Result(Error error) => new(false, error);
}

public record Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error? error, string? member = null, int line = 0)
        : base(isSuccess, error, member, line)
    {
        Value = value;
    }

    public T Value =>
        IsSuccess ? field! : throw new InvalidOperationException("Cannot access value of failure.");

    public static Result<T> Success(T value) => new(value, true, null);

    public static new Result<T> Failure(
        Error error,
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => new(default, false, error, member, line);

    public static implicit operator Result<T>(T value) => new(value, true, null);

    public static implicit operator Result<T>(Error error) => new(default, false, error);
}
