using Application.Abstractions.Database;
using Domain.Carts;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

public class RemoveInvalidPromocodesFromCarts(
    ILogger<RemoveInvalidPromocodesFromCarts> logger,
    IServiceScopeFactory serviceFactory
) : BackgroundService
{
    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));

        do
        {
            await using var scope = serviceFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var promocodeRepository =
                scope.ServiceProvider.GetRequiredService<IPromocodeRepository>();
            var cartRepository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

            await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);

            try
            {
                var invalidPromocodes =
                    await promocodeRepository.GetAndLockInvalidPromocodeIdsAsync(
                        PositiveInt.Create(BatchSize).Value,
                        stoppingToken
                    );

                var promocodes = invalidPromocodes.ToList();
                if (promocodes.Count == 0)
                    continue;

                await cartRepository.RemovePromocodesFromCartsAsync(promocodes, stoppingToken);

                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while removing expired promocodes from carts");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
