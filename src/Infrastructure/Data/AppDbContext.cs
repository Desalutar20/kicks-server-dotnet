using Application.Abstractions.Database;
using Domain.Brands;
using Domain.Brands.Exceptions;
using Domain.Carts.Exceptions;
using Domain.Categories;
using Domain.Categories.Exceptions;
using Domain.Orders.Exceptions;
using Domain.Products.Exceptions;
using Domain.Products.ProductSkus.Exceptions;
using Domain.Promocodes.Exceptions;
using Domain.Users.Exceptions;
using Npgsql;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    private static readonly Dictionary<string, Func<Exception, Exception>> DuplicateExceptions =
        new()
        {
            [DbConstants.UserUniqueIndex] = ex => new UserAlreadyExistsException(ex),
            [DbConstants.BrandUniqueIndex] = ex => new BrandAlreadyExistsException(ex),
            [DbConstants.CategoryUniqueIndex] = ex => new CategoryAlreadyExistsException(ex),
            [DbConstants.ProductUniqueIndex] = ex => new ProductAlreadyExistsException(ex),
            [DbConstants.ProductSkuDuplicateCombinationUniqueIndex] =
                ex => new ProductSkuDuplicateCombinationException(ex),
            [DbConstants.ProductSkuSkuUniqueIndex] = ex => new ProductSkuSkuAlreadyExistsException(
                ex
            ),
            [DbConstants.PromocodeUniqueIndex] = ex => new PromocodeAlreadyExistsException(ex),
            [DbConstants.OrderUserPromocodeUniqueIndex] = ex => new PromocodeAlreadyUsedException(
                ex
            ),
        };

    private static readonly Dictionary<string, Func<Exception, Exception>> ForeignKeyExceptions =
        new()
        {
            [DbConstants.ProductBrandForeignKey] = ex => new BrandDoesNotExistsException(ex),
            [DbConstants.ProductCategoryForeignKey] = ex => new CategoryDoesNotExistsException(ex),
            [DbConstants.ProductSkuProductForeignKey] = ex => new ProductDoesNotExistsException(ex),
            [DbConstants.CartItemProductSkuForeignKey] = ex => new ProductSkuDoesNotExistsException(
                ex
            ),
            [DbConstants.OrderDeliveryOptionForeignKey] =
                ex => new DeliveryOptionDoesNotExistsException(ex),
        };

    public DbSet<DomainUser> Users { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DomainProduct> Products { get; set; }
    public DbSet<ProductSku> ProductSkus { get; set; }
    public DbSet<DomainPromocode> Promocodes { get; set; }
    public DbSet<DomainCart> Carts { get; set; }
    public DbSet<DomainOrder> Orders { get; set; }
    public DbSet<DomainDeliveryOption> DeliveryOptions { get; set; }

    public DbSet<Application.Abstractions.Outbox.Outbox> Outboxes { get; set; }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await Database.BeginTransactionAsync(ct);

        return new EfTransaction(tx);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await base.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is not PostgresException pex)
            {
                throw;
            }

            if (
                TryMapException(
                    pex,
                    DbConstants.UniqueViolationErrorCode,
                    DuplicateExceptions,
                    out var duplicate
                )
            )
            {
                throw duplicate!;
            }

            if (
                TryMapException(
                    pex,
                    DbConstants.ForeignKeyViolationErrorCode,
                    ForeignKeyExceptions,
                    out var foreignKey
                )
            )
            {
                throw foreignKey!;
            }

            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    private static bool TryMapException(
        PostgresException pex,
        string sqlState,
        Dictionary<string, Func<Exception, Exception>> map,
        out Exception? exception
    )
    {
        exception = null;

        if (
            pex.SqlState != sqlState
            || pex.ConstraintName is null
            || !map.TryGetValue(pex.ConstraintName, out var factory)
        )
        {
            return false;
        }

        exception = factory(pex);
        return true;
    }
}
