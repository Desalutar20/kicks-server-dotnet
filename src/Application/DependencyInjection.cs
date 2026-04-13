using Application.Abstractions.OAuth;
using Application.Auth.Types;
using Application.Auth.UseCases.Authenticate;
using Application.Auth.UseCases.DeleteExpiredSessions;
using Application.Auth.UseCases.ForgotPassword;
using Application.Auth.UseCases.GenerateOAuthUrl;
using Application.Auth.UseCases.Logout;
using Application.Auth.UseCases.OAuthSignIn;
using Application.Auth.UseCases.ResetPassword;
using Application.Auth.UseCases.SignIn;
using Application.Auth.UseCases.SignUp;
using Application.Auth.UseCases.VerifyAccount;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<ICommandHandler<SignUpCommand>,
                SignUpCommandHandler>();
        services
            .AddScoped<ICommandHandler<SignInCommand, UserWithSessionId>,
                SignInCommandHandler>();
        services
            .AddScoped<ICommandHandler<VerifyAccountCommand, UserWithSessionId>,
                VerifyAccountCommandHandler>();

        services
            .AddScoped<ICommandHandler<ForgotPasswordCommand>,
                ForgotPasswordCommandHandler>();
        services
            .AddScoped<ICommandHandler<ResetPasswordCommand>,
                ResetPasswordHandler>();

        services
            .AddScoped<ICommandHandler<AuthenticateCommand, SessionUser>,
                AuthenticateCommandHandler>();
        services
            .AddScoped<ICommandHandler<LogoutCommand>,
                LogoutCommandHandler>();
        services
            .AddScoped<ICommandHandler<DeleteExpiredSessionCommand>,
                DeleteExpiredSessionCommandHandler>();

        services
            .AddScoped<ICommandHandler<GenerateOAuthUrlCommand, (Uri, OAuthState)>, GenerateOAuthUrlCommandHandler>();
        services
            .AddScoped<ICommandHandler<OAuthSignInCommand, Guid>, OAuthSignInCommandHandler>();


        return services;
    }
}