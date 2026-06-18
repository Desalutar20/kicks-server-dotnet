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
        RuleFor(x => x.Application.OAuthStateCookieName).NotEmpty();
        RuleFor(x => x.Application.MaxCancelledOrdersPerDay).InclusiveBetween(3, 5);
        RuleFor(x => x.Application.AccountVerificationTtlMinutes).InclusiveBetween(60, 1440);
        RuleFor(x => x.Application.SessionTtlMinutes).InclusiveBetween(1440, 43800);
        RuleFor(x => x.Application.ResetPasswordTtlMinutes).InclusiveBetween(5, 10);
        RuleFor(x => x.Application.OAuthStateTtlMinutes).InclusiveBetween(3, 5);
        RuleFor(x => x.Application.OrderExpirationTtlMinutes)
            .LessThanOrEqualTo(DomainOrder.ExpirationMaxMinutes);
        RuleFor(x => x.Application.OAuthStateTtlMinutes).InclusiveBetween(3, 5);

        RuleFor(x => x.Application.CookieSecure).NotNull();

        RuleFor(x => x.OAuth).NotEmpty();
        RuleFor(x => x.OAuth.Google).NotEmpty();
        RuleFor(x => x.OAuth.Google.ClientId).NotEmpty();
        RuleFor(x => x.OAuth.Google.ClientSecret).NotEmpty();
        RuleFor(x => x.OAuth.Google.RedirectUrl).NotEmpty();
        RuleFor(x => x.OAuth.Facebook).NotEmpty();
        RuleFor(x => x.OAuth.Facebook.ClientId).NotEmpty();
        RuleFor(x => x.OAuth.Facebook.ClientSecret).NotEmpty();
        RuleFor(x => x.OAuth.Facebook.RedirectUrl).NotEmpty();

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

        RuleFor(x => x.Cloudinary).NotEmpty();
        RuleFor(x => x.Cloudinary.ApiKey).NotEmpty();
        RuleFor(x => x.Cloudinary.Secret).NotEmpty();
        RuleFor(x => x.Cloudinary.CloudName).NotEmpty();
        RuleFor(x => x.Cloudinary.Folder).NotEmpty();

        RuleFor(x => x.RateLimit).NotEmpty();
        RuleFor(x => x.RateLimit.SignUp).NotEmpty();
        RuleFor(x => x.RateLimit.SignIn).NotEmpty();
        RuleFor(x => x.RateLimit.VerifyAccount).NotEmpty();
        RuleFor(x => x.RateLimit.GetProfile).NotEmpty();
        RuleFor(x => x.RateLimit.ForgotPassword).NotEmpty();
        RuleFor(x => x.RateLimit.ResetPassword).NotEmpty();
        RuleFor(x => x.RateLimit.Logout).NotEmpty();
        RuleFor(x => x.RateLimit.GetProductSku).NotEmpty();
        RuleFor(x => x.RateLimit.GetProductSkus).NotEmpty();
        RuleFor(x => x.RateLimit.GetProductSkusFilters).NotEmpty();
        RuleFor(x => x.RateLimit.GetCart).NotEmpty();
        RuleFor(x => x.RateLimit.AddCartItem).NotEmpty();
        RuleFor(x => x.RateLimit.UpdateCartItemQuantity).NotEmpty();
        RuleFor(x => x.RateLimit.RemoveCartItem).NotEmpty();
        RuleFor(x => x.RateLimit.ClearCart).NotEmpty();
        RuleFor(x => x.RateLimit.ApplyPromocode).NotEmpty();
        RuleFor(x => x.RateLimit.RemovePromocode).NotEmpty();
        RuleFor(x => x.RateLimit.CreateOrder).NotEmpty();
        RuleFor(x => x.RateLimit.GetOrders).NotEmpty();
        RuleFor(x => x.RateLimit.GetOrder).NotEmpty();
    }
}
