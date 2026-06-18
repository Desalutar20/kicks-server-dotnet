using Application.Abstractions.FileUploader;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks.FileDeleter;

internal sealed class QueueFileDeleter(
    IMessageQueue<IEnumerable<FileUploadResult>> queue,
    IFileUploader fileUploader,
    ILogger<QueueFileDeleter> logger
) : BackgroundService
{
    private const int RetryCount = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var files in queue.ReadAllAsync(stoppingToken))
        {
            var fileUploadResults = files.ToArray();

            for (var i = 0; i < RetryCount; i++)
            {
                try
                {
                    var result = await fileUploader.DeleteFilesAsync(
                        fileUploadResults.Select(f => f.Id).ToArray(),
                        stoppingToken
                    );

                    if (result.IsSuccess)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to delete files. Attempt {Attempt}/{MaxAttempts}",
                        i + 1,
                        RetryCount
                    );
                }
            }
        }
    }
}
