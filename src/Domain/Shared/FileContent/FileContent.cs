namespace Domain.Shared.FileContent;

public abstract record FileContent
{
    public Guid Id { get; protected set; }

    public FileUrl Url { get; protected set; } = null!;

    public FileName Name { get; protected set; } = null!;
}
