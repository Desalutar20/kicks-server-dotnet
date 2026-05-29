namespace Application.Abstractions.OAuth;

public sealed record OAuthUser(ProviderId ProviderId, Domain.Users.Email Email);
