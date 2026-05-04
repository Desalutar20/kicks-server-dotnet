using Domain.Shared;

namespace Domain.User;

public sealed record UsersFilters(
    NonEmptyString? Search,
    Gender? gender,
    bool? IsBanned,
    bool? IsVerified
);
