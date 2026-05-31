using Application.Abstractions.Database;
using Application.Admin.Users.Errors;

namespace Application.Admin.Users.UseCases.ToggleBanUser;

public sealed record ToggleBanUserCommand(UserId UserId) : ICommand;

internal sealed class ToggleBanUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IAuthCache authCache
) : ICommandHandler<ToggleBanUserCommand>
{
    public async Task<Result> Handle(ToggleBanUserCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByIdAsync(command.UserId, true, ct);
        if (user is null)
        {
            return AdminUserErrors.UserNotFound(command.UserId);
        }

        user.ToggleIsBanned();
        if (user.IsBanned)
        {
            await authCache.DeleteAllSessionsAsync(user.Id, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
