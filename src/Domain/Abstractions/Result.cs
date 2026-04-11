using System.Runtime.CompilerServices;

namespace Domain.Abstractions;

public record Result
{
    protected Result(bool isSuccess, Error? error, string? sourceMember = null,
        int sourceLine = 0)
    {
        if (isSuccess && error is not null) throw new InvalidOperationException();
        if (!isSuccess && error is null) throw new InvalidOperationException();

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
        IsFailure ? field! : throw new InvalidOperationException("Cannot access error of success result.");

    public static Result Success() => new(true, null);

    public static Result Failure(
        Error error,
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0) =>
        new(false, error, member, line);
}

public record Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error? error, string? member = null,
        int line = 0)
        : base(isSuccess, error, member, line)
    {
        Value = value;
    }

    public T Value =>
        IsSuccess
            ? field!
            : throw new InvalidOperationException("Cannot access value of failure.");

    public static Result<T> Success(T value) => new(value, true, null);

    public new static Result<T> Failure(Error error, [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0) =>
        new(default, false, error, member, line);

    public Result<TOut> Map<TOut>(Func<T, TOut> func) =>
        IsSuccess
            ? Result<TOut>.Success(func(Value))
            // ReSharper disable once ExplicitCallerInfoArgument
            : Result<TOut>.Failure(Error, SourceMember ?? "", SourceLine);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) =>
        IsSuccess
            ? func(Value)
            // ReSharper disable once ExplicitCallerInfoArgument
            : Result<TOut>.Failure(Error, SourceMember ?? "", SourceLine);
}