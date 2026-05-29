namespace Application.Config;

public sealed record RateLimitConfig
{
    public required int SignUp { get; init; }
    public required int SignIn { get; init; }
    public required int VerifyAccount { get; init; }
    public required int ForgotPassword { get; init; }
    public required int ResetPassword { get; init; }
    public required int GetProfile { get; init; }
    public required int Logout { get; init; }

    public required int GetProductSku { get; init; }
    public required int GetProductSkus { get; init; }
    public required int GetProductSkusFilters { get; init; }
}
