using Domain.Shared;

namespace Domain.Users;

public sealed record UsersFilters(
    NonEmptyString? Search,
    Gender? Gender,
    bool? IsBanned,
    bool? IsVerified
);
