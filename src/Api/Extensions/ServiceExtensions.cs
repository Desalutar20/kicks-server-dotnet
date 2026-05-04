using System.Threading.RateLimiting;
using Api.ExceptionHandler;
using Application.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOpenApi();
        services.AddHttpLogging();
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.TryAdd("requestId", ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
            }
        );

        services.AddCors(op =>
        {
            var applicationConfig = configuration
                .GetRequiredSection("Application")
                .Get<ApplicationConfig>();

            ArgumentNullException.ThrowIfNull(applicationConfig);

            op.AddDefaultPolicy(options =>
            {
                options
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(applicationConfig.ClientUrl)
                    .AllowCredentials();
            });
        });

        services.AddRateLimiter(options =>
        {
            var rateLimitConfig = configuration
                .GetRequiredSection("RateLimit")
                .Get<RateLimitConfig>();

            ArgumentNullException.ThrowIfNull(rateLimitConfig);

            foreach (var property in typeof(RateLimitConfig).GetProperties())
            {
                var value = property.GetValue(rateLimitConfig, null);
                if (value is null)
                    continue;

                options.AddFixedWindowLimiter(
                    property.Name,
                    cfg =>
                    {
                        cfg.PermitLimit = (int)value;
                        cfg.Window = TimeSpan.FromMinutes(1);
                    }
                );
            }

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (ctx, token) =>
            {
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    await ctx.HttpContext.Response.WriteAsJsonAsync(
                        new ProblemDetails
                        {
                            Status = StatusCodes.Status429TooManyRequests,
                            Title = "Too Many Requests",
                            Detail =
                                $"Too many requests. Try again after {retryAfter.TotalSeconds} seconds.",
                            Extensions =
                            {
                                ["requestId"] = ctx.HttpContext.TraceIdentifier,
                                ["timestamp"] = DateTime.UtcNow,
                            },
                        },
                        token
                    );
                }
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
