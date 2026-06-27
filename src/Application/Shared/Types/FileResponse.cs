namespace Application.Shared.Types;

public record FileResponse
{
    public Guid Id { get; init; }
    public required Uri Url { get; init; }
    public required string Name { get; init; }
}
