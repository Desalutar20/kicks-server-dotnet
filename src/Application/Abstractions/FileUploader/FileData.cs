using Domain.Shared.FileContent;
using Domain.Shared.ValueObjects;

namespace Application.Abstractions.FileUploader;

public sealed record FileData(Stream Content, FileName FileName, NonEmptyString ContentType);
