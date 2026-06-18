using Application.Abstractions.Cache;
using Application.Abstractions.Database;
using Application.Abstractions.Events;
using Application.Abstractions.FileUploader;
using Application.Abstractions.Messaging;
using Application.Abstractions.OAuth;
using Application.Abstractions.Outbox;
using Domain.Brands;
using Domain.Carts;
using Domain.Categories;
using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Promocodes;
using Infrastructure.BackgroundTasks;
using Infrastructure.BackgroundTasks.FileDeleter;
using Infrastructure.Cache;
using Infrastructure.Data.Cart;
using Infrastructure.Data.DeliveryOption;
using Infrastructure.Data.Order;
using Infrastructure.Data.Outbox;
using Infrastructure.Data.Product;
using Infrastructure.Data.Promocode;
using Infrastructure.Data.User;
using Infrastructure.Events;
using Infrastructure.MessageQueue;
using Infrastructure.Services;
using Infrastructure.Services.OAuth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services
                .AddConfig(configuration)
                .AddDatabase()
                .AddRedis()
                .AddRepositories()
                .AddServices()
                .AddHostedServices();

            services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

            return services;
        }

        private IServiceCollection AddConfig(IConfiguration configuration)
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

            return services;
        }

        private IServiceCollection AddDatabase()
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

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

            return services;
        }

        private IServiceCollection AddRedis()
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

            return services;
        }

        private IServiceCollection AddServices()
        {
            services.AddSingleton<IHashingService, HashingService>();
            services.AddSingleton<IEmailSender, EmailService>();
            services.AddSingleton<IFileUploader, FileUploaderService>();
            services.AddSingleton<ICachingService, Redis>();
            services.AddSingleton<IAuthCache, AuthCache>();
            services.AddKeyedSingleton<IOAuthClient, GoogleService>(OAuthProvider.Google);
            services.AddKeyedSingleton<IOAuthClient, FacebookService>(OAuthProvider.Facebook);
            services.AddSingleton<IOAuthClientFactory, OAuthClientFactory>();

            services.AddHttpClient<GoogleService>(client =>
                client.Timeout = TimeSpan.FromSeconds(10)
            );
            services.AddHttpClient<FacebookService>(client =>
                client.Timeout = TimeSpan.FromSeconds(10)
            );

            services.AddSingleton<
                IMessageQueue<IEnumerable<FileUploadResult>>,
                InMemoryMessageQueue<IEnumerable<FileUploadResult>>
            >();

            return services;
        }

        private IServiceCollection AddRepositories()
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductSkusRepository, ProductSkusRepository>();
            services.AddScoped<IPromocodeRepository, PromocodeRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IDeliveryOptionRepository, DeliveryOptionRepository>();

            services.AddScoped<IOutboxRepository, OutboxRepository>();

            return services;
        }

        private IServiceCollection AddHostedServices()
        {
            services.AddHostedService<OutboxEmailSender>();
            services.AddHostedService<OutboxFileDeleter>();
            services.AddHostedService<QueueFileDeleter>();
            services.AddHostedService<DeleteExpiredSessions>();
            services.AddHostedService<RemoveInvalidPromocodesFromCarts>();
            services.AddHostedService<CancelExpiredOrders>();

            return services;
        }
    }
}
