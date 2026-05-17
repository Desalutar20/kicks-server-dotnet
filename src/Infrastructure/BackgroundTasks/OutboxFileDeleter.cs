using System.Text.Json;
using Application.Abstractions.FileUploader;
using Domain.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks;

internal sealed class OutboxFileDeleter(
    ILogger<OutboxFileDeleter> logger,
    IServiceScopeFactory serviceFactory
) : BackgroundService
{
    private const int BatchSize = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        do
        {
            Console.WriteLine("PROCESSING FILE OUTBOX\n\n\n\n\n\n");
            await using var scope = serviceFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var fileUploader = scope.ServiceProvider.GetRequiredService<IFileUploader>();

            await unitOfWork.BeginTransactionAsync(
                async () =>
                {
                    try
                    {
                        var outboxes = await outboxRepository.GetAndLockOutboxesForProcessingAsync(
                            OutboxType.File,
                            PositiveInt.Create(BatchSize).Value,
                            false,
                            stoppingToken
                        );

                        var processedIds = new List<OutboxId>(BatchSize);

                        foreach (var outbox in outboxes)
                            try
                            {
                                var fileIds = JsonSerializer.Deserialize<List<Guid>>(
                                    outbox.Data.Value
                                );

                                if (fileIds is null || fileIds.Count == 0)
                                {
                                    logger.LogWarning("Empty file outbox {OutboxId}", outbox.Id);
                                    processedIds.Add(outbox.Id);
                                    continue;
                                }

                                await fileUploader.DeleteFilesAsync(fileIds, stoppingToken);

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

                            await outboxRepository.MarkAsProcessedAsync(
                                processedIds,
                                now,
                                stoppingToken
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Error while processing file outbox");
                    }

                    await Task.CompletedTask;
                },
                stoppingToken
            );
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
