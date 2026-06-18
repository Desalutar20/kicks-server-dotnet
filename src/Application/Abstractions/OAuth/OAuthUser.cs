namespace Application.Abstractions.OAuth;

public sealed record OAuthUser(ProviderId ProviderId, Domain.Shared.ValueObjects.Email Email);
