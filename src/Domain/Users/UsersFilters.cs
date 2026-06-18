using Domain.Shared;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed record UsersFilters(
    NonEmptyString? Search,
    Gender? Gender,
    bool? IsBanned,
    bool? IsVerified
);
