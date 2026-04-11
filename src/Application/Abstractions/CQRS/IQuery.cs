namespace Application.Abstractions.CQRS;

public interface IQuery<out TResponse> where TResponse : notnull;

public interface IQuery;