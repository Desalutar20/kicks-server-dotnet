using Application.Abstractions.Cache;
using Application.Abstractions.Events;
using Domain.Outbox;
using Infrastructure.BackgroundTasks;
using Infrastructure.Cache;
using Infrastructure.Data.Outbox;
using Infrastructure.Data.User;
using Infrastructure.Events;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddConfig(services, configuration);
        AddDatabase(services);
        AddRedis(services);

        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<IEmailSender, EmailService>();
        services.AddSingleton<ICachingService, Redis>();
        services.AddSingleton<IAuthCache, AuthCache>();

        services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddHostedService<OutboxEmailSender>();
        services.AddHostedService<DeleteExpiredSessions>();

        return services;
    }

    private static void AddConfig(IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<Config>()
            .Bind(configuration)
            .Validate(x =>
            {
                ConfigValidator validator = new();

                var result = validator.Validate(x);
                if (result.IsValid) return true;

                foreach (var error in result.Errors)
                    Console.WriteLine(
                        $"[CONFIG ERROR] Property: {error.PropertyName}, Error: {error.ErrorMessage}, Attempted Value: {error.AttemptedValue}");

                return result.IsValid;
            });

        services.AddSingleton<Config>(sp => sp.GetRequiredService<IOptions<Config>>().Value);
    }

    private static void AddDatabase(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, DateInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, PublishDomainEventInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<Config>();
            options
                .UseNpgsql(cfg.Database.GetConnectionString())
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                .UseSnakeCaseNamingConvention();
        });
    }

    private static void AddRedis(IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<Config>();

            ConfigurationOptions options =
                new()
                {
                    EndPoints = { $"{config.Redis.Host}:{config.Redis.Port}" },
                    User = string.IsNullOrEmpty(config.Redis.User)
                        ? null
                        : config.Redis.User,
                    Password = config.Redis.Password,
                    DefaultDatabase = config.Redis.Database
                };

            return ConnectionMultiplexer.Connect(options);
        });
    }
}