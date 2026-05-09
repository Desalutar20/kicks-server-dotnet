using Domain.Shared;

namespace Domain.User;

public sealed record UsersFilters(
    NonEmptyString? Search,
    Gender? Gender,
    bool? IsBanned,
    bool? IsVerified
);
