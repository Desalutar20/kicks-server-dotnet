namespace Application.Config;

public sealed record RateLimitConfig
{
    public required int SignUp { get; set; }
    public required int SignIn { get; set; }
    public required int VerifyAccount { get; set; }
    public required int ForgotPassword { get; set; }
    public required int ResetPassword { get; set; }
    public required int GetProfile { get; set; }
    public required int Logout { get; set; }
}