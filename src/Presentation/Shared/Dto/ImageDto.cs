using Domain.Shared.FileContent;

namespace Presentation.Shared.Dto;

public record FileDto(Guid Id, string Url, string Name);

internal static class ImageDtoMapper
{
    public static FileDto ToDto(this FileContent model) =>
        new(model.Id, model.Url.Value, model.Name.FullName);
}
