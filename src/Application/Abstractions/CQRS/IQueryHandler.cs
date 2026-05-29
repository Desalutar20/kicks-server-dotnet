namespace Application.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct = default);
}
