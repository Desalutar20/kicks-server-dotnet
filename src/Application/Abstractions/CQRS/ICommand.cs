namespace Application.Abstractions.CQRS;

public interface ICommand<out TResponse> where TResponse : notnull;

public interface ICommand;