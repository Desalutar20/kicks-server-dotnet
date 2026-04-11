namespace Application.Abstractions.Cache;

public interface ICachingService
{
    Task SetAsync(NonEmptyString key, NonEmptyString data, TimeSpan? exp, CancellationToken ct = default);
    Task<string?> GetAsync(NonEmptyString key, TimeSpan? exp, CancellationToken ct = default);
    Task DeleteAsync(NonEmptyString key, CancellationToken ct = default);
    Task<string?> GetDelAsync(NonEmptyString key, CancellationToken ct = default);
    Task<bool> ExistsAsync(NonEmptyString key, CancellationToken ct = default);
    Task RefreshAsync(NonEmptyString key, TimeSpan exp, CancellationToken ct = default);
}