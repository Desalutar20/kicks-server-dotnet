using Domain.Product.Brand.Exceptions;
using Domain.Product.Category.Exceptions;
using Domain.Product.Exceptions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Data.Shared;

internal sealed class UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private static readonly Dictionary<string, Func<Exception, Exception>> DuplicateExceptions =
        new()
        {
            [DbConstants.BrandUniqueIndex] = (ex) => new BrandAlreadyExistsException(ex),
            [DbConstants.CategoryUniqueIndex] = (ex) => new CategoryAlreadyExistsException(ex),
            [DbConstants.ProductUniqueIndex] = (ex) => new ProductAlreadyExistsException(ex),
        };

    private static readonly Dictionary<string, Func<Exception, Exception>> ForeignKeyExceptions =
        new()
        {
            ["fk_product_brand_brand_id"] = (ex) => new BrandDoesNotExistsException(ex),
            ["fk_product_category_category_id"] = (ex) => new CategoryDoesNotExistsException(ex),
        };

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is not PostgresException pex)
                throw;

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

    public async Task BeginTransactionAsync(Func<Task> func, CancellationToken ct = default)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            await func();
            await tx.CommitAsync(ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Transaction failed");
            await tx.RollbackAsync(ct);
            throw;
        }
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
