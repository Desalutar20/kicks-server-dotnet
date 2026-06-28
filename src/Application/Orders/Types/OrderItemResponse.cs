namespace Application.Orders.Types;

public sealed record OrderItemResponse
{
    public Guid Id { get; init; }

    public decimal Price { get; init; }

    public required string Title { get; init; }
    public required string Description { get; init; }

    public int Size { get; init; }
    public int Quantity { get; init; }

    public required string Image { get; init; }
}
