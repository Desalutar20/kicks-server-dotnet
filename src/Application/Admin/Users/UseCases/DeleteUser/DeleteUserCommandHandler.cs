using Application.Admin.Users.Errors;

namespace Application.Admin.Users.UseCases.DeleteUser;

public sealed record DeleteUserCommand(UserId UserId) : ICommand;

internal sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByIdAsync(command.UserId, true, ct);
        if (user is null)
        {
            return AdminUserErrors.UserNotFound(command.UserId);
        }

        userRepository.DeleteUser(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
