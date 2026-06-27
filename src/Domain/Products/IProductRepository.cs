namespace Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(
        ProductId productId,
        bool trackChanges,
        CancellationToken ct = default
    );

    void CreateProduct(Product product);
    void UpdateProduct(Product product);
    void DeleteProduct(Product product);
}
