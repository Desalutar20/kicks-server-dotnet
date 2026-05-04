using Microsoft.Extensions.DependencyInjection;
using Presentation.Admin;
using Presentation.Auth.Endpoints;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1");

        group.MapAuthV1();
        group.MapAdminV1();

        return app;
    }
}
