namespace Application.Abstractions.OAuth;

public sealed record OAuthUser(ProviderId ProviderId, Domain.User.Email Email);