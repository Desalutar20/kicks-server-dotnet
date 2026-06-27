namespace Application.Config;

public sealed record StripeConfig
{
    public required string Secret { get; init; }
};
