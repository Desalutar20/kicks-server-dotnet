namespace Domain.Abstractions;

public enum ErrorType
{
    Failure = 0,
    NotFound = 1,
    Validation = 2,
    Conflict = 3,
    Unauthorized = 4,
    AccessForbidden = 5,
    Internal = 6,
}
