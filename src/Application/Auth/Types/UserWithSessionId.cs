namespace Application.Auth.Types;

public sealed record UserWithSessionId(SessionUser User, Guid? SessionId);