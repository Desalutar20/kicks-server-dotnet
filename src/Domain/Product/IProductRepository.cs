using Domain.Shared.Pagination;

namespace Domain.Product;

public interface IProductRepository
{
    Task<KeysetPaginated<Product, ProductId>> GetProductsAsync(
        ProductFilters filters,
        KeysetPagination<ProductId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Product?> GetProductByIdAsync(
        ProductId productId,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<ProductFilterOptions> GetProductsFilterOptions(
        bool trackChanges,
        CancellationToken ct = default
    );

    void CreateProduct(Product product);
    void UpdateProduct(Product product);
    void DeleteProduct(Product product);
}
