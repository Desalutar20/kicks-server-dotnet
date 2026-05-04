namespace Domain.Abstractions;

public abstract class Entity<T>(T id) : IEntity<T>
{
    public T Id { get; init; } = id;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
