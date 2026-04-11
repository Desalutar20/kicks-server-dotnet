using Application.Auth.Types;
using Application.Auth.UseCases.Authenticate;
using Application.Auth.UseCases.DeleteExpiredSessions;
using Application.Auth.UseCases.ForgotPassword;
using Application.Auth.UseCases.Logout;
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
            .AddScoped<ICommandHandler<SignUpCommand, Result>,
                SignUpCommandHandler>();
        services
            .AddScoped<ICommandHandler<SignInCommand, Result<UserWithSessionId>>,
                SignInCommandHandler>();
        services
            .AddScoped<ICommandHandler<VerifyAccountCommand, Result<UserWithSessionId>>,
                VerifyAccountCommandHandler>();

        services
            .AddScoped<ICommandHandler<ForgotPasswordCommand, Result>,
                ForgotPasswordCommandHandler>();
        services
            .AddScoped<ICommandHandler<ResetPasswordCommand, Result>,
                ResetPasswordHandler>();

        services
            .AddScoped<ICommandHandler<AuthenticateCommand, Result<SessionUser>>,
                AuthenticateCommandHandler>();
        services
            .AddScoped<ICommandHandler<LogoutCommand>,
                LogoutCommandHandler>();
        services
            .AddScoped<ICommandHandler<DeleteExpiredSessionCommand>,
                DeleteExpiredSessionCommandHandler>();


        return services;
    }
}