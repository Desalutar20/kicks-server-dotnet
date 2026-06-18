using System.Text.Json;
using Application.Abstractions.Database;
using Application.Abstractions.FileUploader;
using Application.Abstractions.Outbox;
using Domain.Shared.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks.FileDeleter;

internal sealed class OutboxFileDeleter(
    ILogger<OutboxFileDeleter> logger,
    IFileUploader fileUploader,
    IServiceScopeFactory serviceFactory
) : BackgroundService
{
    private const int BatchSize = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        do
        {
            await using var scope = serviceFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

            await using var transaction = await unitOfWork.BeginTransactionAsync(stoppingToken);

            try
            {
                var outboxes = await outboxRepository.GetAndLockOutboxesForProcessingAsync(
                    OutboxType.File,
                    PositiveInt.Create(BatchSize).Value,
                    stoppingToken
                );

                var processedIds = new List<OutboxId>(BatchSize);

                foreach (var outbox in outboxes)
                    try
                    {
                        var fileIds = JsonSerializer.Deserialize<List<Guid>>(outbox.Data.Value);
                        if (fileIds is null || fileIds.Count == 0)
                        {
                            logger.LogWarning("Empty file outbox {OutboxId}", outbox.Id);
                            processedIds.Add(outbox.Id);
                            continue;
                        }

                        var result = await fileUploader.DeleteFilesAsync(fileIds, stoppingToken);
                        if (result.IsFailure)
                            continue;

                        processedIds.Add(outbox.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Unexpected error while processing Outbox {OutboxId}",
                            outbox.Id
                        );
                    }

                if (processedIds.Count > 0)
                {
                    var now = DateTimeOffset.UtcNow;

                    await outboxRepository.MarkAsProcessedAsync(processedIds, now, stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while processing file outbox");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
