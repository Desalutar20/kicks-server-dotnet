namespace Domain.Abstractions;

public interface IEntity<T> : IEntity
{
    public T Id { get; init; }
}

public interface IEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}