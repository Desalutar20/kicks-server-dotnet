using Application.Abstractions.Database;
using Domain.Orders;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

internal sealed class CancelExpiredOrders(
    IServiceScopeFactory serviceFactory,
    ILogger<CancelExpiredOrders> logger
) : BackgroundService
{
    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        do
        {
            await using var scope = serviceFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var promocodeRepository =
                scope.ServiceProvider.GetRequiredService<IPromocodeRepository>();
            var productSkuRepository =
                scope.ServiceProvider.GetRequiredService<IProductSkusRepository>();

            await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);

            try
            {
                var expiredOrders = await orderRepository.GetAndLockExpiredOrdersAsync(
                    PositiveInt.Create(BatchSize).Value,
                    stoppingToken
                );

                var orders = expiredOrders.ToList();
                if (orders.Count == 0)
                    continue;

                await Task.WhenAll([
                    orderRepository.BulkUpdateOrdersStatusAsync(
                        orders.Select(o => o.Id).ToList(),
                        OrderStatus.Cancelled,
                        stoppingToken
                    ),
                    promocodeRepository.BulkDecrementUsageCountAsync(
                        orders.Select(o => o.PromocodeId).Where(x => x is not null).ToList()!,
                        stoppingToken
                    ),
                    productSkuRepository.BulkIncrementQuantityAsync(
                        orders.SelectMany(o => o.Items).ToList(),
                        stoppingToken
                    ),
                ]);

                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while canceling expired orders");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
