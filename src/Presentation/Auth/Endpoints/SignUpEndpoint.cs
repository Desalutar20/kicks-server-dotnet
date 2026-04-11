using Application.Auth.UseCases.SignUp;

namespace Presentation.Auth.Endpoints;

[Serializable]
public sealed record SignUpRequest(string Email, string Password, string FirstName, string LastName, string Gender);

public sealed class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Email.MaxLength);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(Password.MinLength).MaximumLength(Password.MaxLength);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(FirstName.MaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(LastName.MaxLength);
        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => Enum.TryParse<Gender>(g, true, out _))
            .WithMessage("'Gender' must be one of Male, Female, Other");
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder SignUpV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/sign-up",
                    async (SignUpRequest request,
                        [FromServices] ICommandHandler<SignUpCommand, Result> commandHandler,
                        ILoggerFactory loggerFactory,
                        CancellationToken ct = default) =>
                    {
                        var logger = loggerFactory.CreateLogger("Auth.SignUp");

                        var commandResult = request.ToCommand();
                        if (commandResult.IsFailure) return ErrorHandler.Handle(commandResult.Error, logger);

                        var result = await commandHandler.Handle(commandResult.Value, ct);

                        return result.IsFailure
                            ? ErrorHandler.Handle(result.Error, logger)
                            : Results.Created("/sign-up",
                                new ApiResponse<string>("If the email is registered, instructions have been sent"));
                    })
                .AddEndpointFilter<ValidationFilter>()
                .Produces<ApiResponse<string>>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesValidationProblem()
                .WithName("SignUp")
                .WithSummary("Registers a new user")
                .WithDescription("Creates a user with email, password, first name, last name, and gender")
                .RequireRateLimiting(RateLimitConstants.SignUp);

        return endpoint;
    }

    private static Result<SignUpCommand> ToCommand(this SignUpRequest request)
    {
        var email = Email.Create(request.Email);
        if (email.IsFailure) return Result<SignUpCommand>.Failure(email.Error);

        var password = Password.Create(request.Password);
        if (password.IsFailure)
            return Result<SignUpCommand>.Failure(
                password.Error);

        var firstName = FirstName.Create(request.FirstName);
        if (firstName.IsFailure)
            return Result<SignUpCommand>.Failure(
                firstName.Error);

        var lastName = LastName.Create(request.LastName);
        if (lastName.IsFailure)
            return Result<SignUpCommand>.Failure(
                lastName.Error);

        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            return Result<SignUpCommand>.Failure(
                Error.Validation("gender", ["Invalid gender"]));

        return Result<SignUpCommand>.Success(
            new SignUpCommand(email.Value, password.Value, firstName.Value,
                lastName.Value, gender));
    }
}