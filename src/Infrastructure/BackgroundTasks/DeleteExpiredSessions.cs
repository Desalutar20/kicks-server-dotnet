using Application.Abstractions.CQRS;
using Application.Auth.UseCases.DeleteExpiredSessions;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundTasks;

public class DeleteExpiredSessions(IServiceScopeFactory serviceFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(10));

        do
        {
            await using var scope = serviceFactory.CreateAsyncScope();
            var commandHandler = scope.ServiceProvider.GetRequiredService<
                ICommandHandler<DeleteExpiredSessionCommand>
            >();

            await commandHandler.Handle(new DeleteExpiredSessionCommand(), stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
