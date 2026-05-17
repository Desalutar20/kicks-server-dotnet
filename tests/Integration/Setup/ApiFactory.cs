using Application.Config;
using CloudinaryDotNet;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Integration.Setup;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public override async ValueTask DisposeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<Config>().Cloudinary;

        var cloudinary = new Cloudinary(
            new Account(config.CloudName, config.ApiKey, config.Secret)
        );

        await dbContext.Database.EnsureDeletedAsync();
        await cloudinary.DeleteAllResourcesAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logger => logger.ClearProviders());

        builder.ConfigureAppConfiguration(
            (_, configBuilder) =>
            {
                var guid = Guid.NewGuid();
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Database:Name"] = $"test-{guid}",

                        ["Redis:KeyPrefix"] = $"{guid}:",

                        ["Cloudinary:Folder"] = "kicks-test",

                        ["RateLimit:SignUp"] = "30",
                        ["RateLimit:SignIn"] = "30",
                        ["RateLimit:VerifyAccount"] = "30",
                        ["RateLimit:ForgotPassword"] = "20",
                        ["RateLimit:ResetPassword"] = "20",
                    }
                );
            }
        );

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(
                (sp, opt) =>
                {
                    var cfg = sp.GetRequiredService<Config>();
                    opt.UseNpgsql(cfg.Database.GetConnectionString())
                        .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                        .UseSnakeCaseNamingConvention();
                }
            );

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureCreated();
        });

        base.ConfigureWebHost(builder);
    }
}
