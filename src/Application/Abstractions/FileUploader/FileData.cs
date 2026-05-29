using Domain.Shared.FileContent;

namespace Application.Abstractions.FileUploader;

public sealed record FileData(Stream Content, FileName FileName, NonEmptyString ContentType);
