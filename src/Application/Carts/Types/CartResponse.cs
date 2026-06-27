namespace Application.Carts.Types;

public sealed record CartResponse
{
    public List<CartItemResponse> Items { get; set; } = [];
    public PromocodeResponse? Promocode { get; set; }
    public decimal TotalPrice { get; init; }
};
