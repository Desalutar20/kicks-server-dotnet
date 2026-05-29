namespace Presentation.Shared.Validator;

public static class CustomValidator
{
    public static IRuleBuilderOptionsConditions<T, TElement> ValidateValueObject<
        T,
        TElement,
        TValueObject
    >(this IRuleBuilder<T, TElement> ruleBuilder, Func<TElement, Result<TValueObject>> func)
    {
        return ruleBuilder.Custom(
            (value, context) =>
            {
                var result = func(value);

                if (result.IsSuccess)
                    return;

                AddErrors(context, result);
            }
        );
    }

    public static IRuleBuilderOptionsConditions<T, TElement?> ValidateNullableValueObject<
        T,
        TElement,
        TValueObject
    >(this IRuleBuilder<T, TElement?> ruleBuilder, Func<TElement, Result<TValueObject>> func)
    {
        return ruleBuilder.Custom(
            (value, context) =>
            {
                if (value is null)
                    return;

                var result = func(value);

                if (result.IsSuccess)
                    return;

                AddErrors(context, result);
            }
        );
    }

    public static IRuleBuilderOptionsConditions<T, IEnumerable<TElement>?> ValidateEachValueObject<
        T,
        TElement,
        TValueObject
    >(
        this IRuleBuilder<T, IEnumerable<TElement>?> ruleBuilder,
        Func<TElement, Result<TValueObject>> func
    )
    {
        return ruleBuilder.Custom(
            (value, context) =>
            {
                if (value is null)
                    return;

                foreach (var element in value)
                {
                    var result = func(element);

                    if (result.IsSuccess)
                        continue;

                    AddErrors(context, result);
                    break;
                }
            }
        );
    }

    private static void AddErrors<T>(ValidationContext<T> context, Result result)
    {
        if (result.Error is { ErrorType: ErrorType.Validation, Errors: not null })
        {
            foreach (var error in result.Error.Errors.Value.Item2)
            {
                context.AddFailure(error);
            }

            return;
        }

        context.AddFailure(result.Error.Description);
    }
}
