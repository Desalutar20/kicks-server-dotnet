namespace Application.Config;

public sealed record OAuthConfig
{
    public required OAuthProviderConfig Facebook { get; init; }
    public required OAuthProviderConfig Google { get; init; }
}

public sealed record OAuthProviderConfig
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUrl { get; init; }
}
