using System.Collections.Concurrent;

namespace Presentation.Shared.Filters;

internal sealed class ValidationFilter(IServiceProvider services) : IEndpointFilter
{
    private static readonly ConcurrentDictionary<Type, IValidator?> ValidatorCache = new();

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        foreach (var arg in context.Arguments)
        {
            if (arg is null)
                continue;

            var type = arg.GetType();

            if (!ValidatorCache.TryGetValue(type, out var validator))
            {
                var validatorType = typeof(IValidator<>).MakeGenericType(type);
                validator = services.GetService(validatorType) as IValidator;

                if (validator is not null)
                    ValidatorCache[type] = validator;
            }

            if (validator is null)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(arg),
                context.HttpContext.RequestAborted
            );

            if (result.IsValid)
                continue;

            var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var error in result.Errors)
            {
                var key = error.PropertyName;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                key = char.ToLowerInvariant(key[0]) + key[1..];

                if (!errors.TryGetValue(key, out var list))
                {
                    errors[key] = [error.ErrorMessage];
                    continue;
                }

                var newArray = new string[list.Length + 1];
                Array.Copy(list, newArray, list.Length);
                newArray[^1] = error.ErrorMessage;

                errors[key] = newArray;
            }

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}
