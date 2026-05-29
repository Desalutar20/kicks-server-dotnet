using Domain.Shared.FileContent;

namespace Application.Abstractions.FileUploader;

public sealed record FileUploadResult(
    Guid Id,
    FileUrl Uri,
    FileName FileName,
    long Size,
    NonEmptyString ContentType
);
