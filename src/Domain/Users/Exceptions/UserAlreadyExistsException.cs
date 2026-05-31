using Domain.Abstractions;

namespace Domain.Users.Exceptions;

public sealed class UserAlreadyExistsException(Exception innerException)
    : AppException("User already exists.", innerException);
