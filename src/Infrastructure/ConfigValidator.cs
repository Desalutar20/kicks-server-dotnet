namespace Infrastructure;

internal sealed class ConfigValidator : AbstractValidator<Config>
{
    public ConfigValidator()
    {
        RuleFor(x => x.Application).NotEmpty();
        RuleFor(x => x.Application.ClientUrl).NotEmpty().Url();
        RuleFor(x => x.Application.AccountVerificationPath).NotEmpty();
        RuleFor(x => x.Application.ResetPasswordPath).NotEmpty();
        RuleFor(x => x.Application.SessionCookieName).NotEmpty();
        RuleFor(x => x.Application.AccountVerificationTtlMinutes).InclusiveBetween(60, 1440);
        RuleFor(x => x.Application.SessionTtlMinutes).InclusiveBetween(1440, 43800);
        RuleFor(x => x.Application.ResetPasswordTtlMinutes).InclusiveBetween(5, 10);
        RuleFor(x => x.Application.CookieSecure).NotNull();

        RuleFor(x => x.Database).NotEmpty();
        RuleFor(x => x.Database.Host).NotEmpty();
        RuleFor(x => x.Database.Password).NotEmpty();
        RuleFor(x => x.Database.Name).NotEmpty();
        RuleFor(x => x.Database.User).NotEmpty();
        RuleFor(x => x.Database.Port).NotEmpty().InclusiveBetween(1, 65535);
        RuleFor(x => x.Database.Ssl).NotNull();

        RuleFor(x => x.Redis).NotEmpty();
        RuleFor(x => x.Redis.Host).NotEmpty();
        RuleFor(x => x.Redis.Password).NotNull();
        RuleFor(x => x.Redis.Database).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Redis.User).NotNull();
        RuleFor(x => x.Redis.Port).NotEmpty().InclusiveBetween(1, 65535);

        RuleFor(x => x.Smtp).NotEmpty();
        RuleFor(x => x.Smtp.Host).NotEmpty();
        RuleFor(x => x.Smtp.Password).NotEmpty();
        RuleFor(x => x.Smtp.User).NotEmpty();
        RuleFor(x => x.Smtp.Port).NotEmpty().InclusiveBetween(1, 65535);
        RuleFor(x => x.Smtp.From).NotEmpty();


        RuleFor(x => x.RateLimit).NotEmpty();
        RuleFor(x => x.RateLimit.SignUp).NotEmpty();
        RuleFor(x => x.RateLimit.SignIn).NotEmpty();
        RuleFor(x => x.RateLimit.VerifyAccount).NotEmpty();
        RuleFor(x => x.RateLimit.GetProfile).NotEmpty();
        RuleFor(x => x.RateLimit.ForgotPassword).NotEmpty();
        RuleFor(x => x.RateLimit.Logout).NotEmpty();
    }
}