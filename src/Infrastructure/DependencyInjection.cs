using Application.Abstractions.Cache;
using Application.Abstractions.Events;
using Application.Abstractions.OAuth;
using Domain.Outbox;
using Domain.Product;
using Domain.Product.Brand;
using Domain.Product.Category;
using Infrastructure.BackgroundTasks;
using Infrastructure.Cache;
using Infrastructure.Data.Outbox;
using Infrastructure.Data.Product;
using Infrastructure.Data.User;
using Infrastructure.Events;
using Infrastructure.Services;
using Infrastructure.Services.OAuth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AddConfig(services, configuration);
        AddDatabase(services);
        AddRedis(services);

        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<IEmailSender, EmailService>();
        services.AddSingleton<ICachingService, Redis>();
        services.AddSingleton<IAuthCache, AuthCache>();
        // services.AddKeyedSingleton<IOAuthClient, GoogleService>(OAuthProvider.Google);
        // services.AddKeyedSingleton<IOAuthClient, FacebookService>(OAuthProvider.Facebook);
        services.AddHttpClient<IOAuthClient, GoogleService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHttpClient<IOAuthClient, FacebookService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IOAuthClientFactory, OAuthClientFactory>();

        // services.AddHttpClient<GoogleService>(client =>
        // {
        //     client.Timeout = TimeSpan.FromSeconds(10);
        // });
        // services.AddHttpClient<FacebookService>(client =>
        // {
        //     client.Timeout = TimeSpan.FromSeconds(10);
        // });

        services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddHostedService<OutboxEmailSender>();
        services.AddHostedService<DeleteExpiredSessions>();

        return services;
    }

    private static void AddConfig(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<Config>()
            .Bind(configuration)
            .Validate(x =>
            {
                ConfigValidator validator = new();

                var result = validator.Validate(x);
                if (result.IsValid)
                {
                    return true;
                }

                foreach (var error in result.Errors)
                    Console.WriteLine(
                        $"[CONFIG ERROR] Property: {error.PropertyName}, Error: {error.ErrorMessage}, Attempted Value: {error.AttemptedValue}"
                    );

                return result.IsValid;
            });

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<Config>>().Value);
    }

    private static void AddDatabase(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, DateInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, PublishDomainEventInterceptor>();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var cfg = sp.GetRequiredService<Config>();
                options
                    .UseNpgsql(cfg.Database.GetConnectionString())
                    .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                    .UseSnakeCaseNamingConvention();
            }
        );
    }

    private static void AddRedis(IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<Config>();

            ConfigurationOptions options = new()
            {
                EndPoints = { $"{config.Redis.Host}:{config.Redis.Port}" },
                User = string.IsNullOrEmpty(config.Redis.User) ? null : config.Redis.User,
                Password = config.Redis.Password,
                DefaultDatabase = config.Redis.Database,
            };

            return ConnectionMultiplexer.Connect(options);
        });
    }
}
