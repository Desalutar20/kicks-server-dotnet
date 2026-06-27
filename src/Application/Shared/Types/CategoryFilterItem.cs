namespace Application.Shared.Types;

public sealed record CategoryFilterItem
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
