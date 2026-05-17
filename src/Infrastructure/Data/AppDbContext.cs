using Domain.Brand;
using Domain.Category;
using Domain.Product.ProductSku;
using Domain.Product.ProductSku.ProductSkuImage;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DomainUser> Users { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DomainProduct> Products { get; set; }
    public DbSet<ProductSku> ProductSkus { get; set; }
    public DbSet<ProductSkuImage> ProductSkuImages { get; set; }
    public DbSet<DomainOutbox> Outboxes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
