namespace Application.Abstractions.CQRS;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse> where TResponse : notnull
{
    Task<TResponse> Handle(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken ct = default);
}