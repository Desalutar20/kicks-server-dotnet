using Microsoft.Extensions.DependencyInjection;
using Presentation.Admin;
using Presentation.Auth.Endpoints;
using Presentation.Cart.Endpoints;
using Presentation.DeliveryOptions.Endpoints;
using Presentation.Orders.Endpoints;
using Presentation.ProductSkus.Endpoints;

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
        group.MapProductSkusV1();
        group.MapCartV1();
        group.MapDeliveryOptionsV1();
        group.MapOrdersV1();

        group.MapAdminV1();

        return app;
    }
}
