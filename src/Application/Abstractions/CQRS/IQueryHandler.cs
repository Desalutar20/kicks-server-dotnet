namespace Application.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct = default);
}

public interface IQueryHandler<in TQuery> where TQuery : IQuery
{
    Task Handle(TQuery query, CancellationToken ct = default);
}