using Domain.Brands;
using Domain.Categories;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DomainUser> Users { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DomainProduct> Products { get; set; }
    public DbSet<ProductSku> ProductSkus { get; set; }
    public DbSet<Application.Abstractions.Outbox.Outbox> Outboxes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
