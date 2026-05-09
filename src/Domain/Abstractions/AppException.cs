namespace Domain.Abstractions;

[Serializable]
public abstract class AppException : Exception
{
    protected AppException() { }

    protected AppException(string message)
        : base(message) { }

    protected AppException(string message, Exception innerException)
        : base(message, innerException) { }
}
