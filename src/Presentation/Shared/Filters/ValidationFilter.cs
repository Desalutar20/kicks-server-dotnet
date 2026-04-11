using System.Collections.Concurrent;

namespace Presentation.Shared.Filters;

internal sealed class ValidationFilter(IServiceProvider services) : IEndpointFilter
{
    private static readonly ConcurrentDictionary<Type, IValidator?> ValidatorCache = new();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var arg in context.Arguments)
        {
            if (arg is null) continue;

            var type = arg.GetType();

            if (!ValidatorCache.TryGetValue(type, out var validator))
            {
                var validatorType = typeof(IValidator<>).MakeGenericType(type);
                validator = services.GetService(validatorType) as IValidator;

                if (validator is not null) ValidatorCache[type] = validator;
            }


            if (validator is null) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(arg),
                context.HttpContext.RequestAborted);

            if (result.IsValid) continue;

            var errors = result.Errors
                               .GroupBy(e => e.PropertyName)
                               .ToDictionary(
                                   g => char.ToLowerInvariant(g.Key[0]) + g.Key[1..],
                                   g => g.Select(e => e.ErrorMessage).ToArray()
                               );

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}