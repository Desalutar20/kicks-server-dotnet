namespace Application.Config;

public sealed record CloudinaryConfig
{
    public required string ApiKey { get; init; }
    public required string Secret { get; init; }
    public required string CloudName { get; init; }
    public required string Folder { get; init; }
};
