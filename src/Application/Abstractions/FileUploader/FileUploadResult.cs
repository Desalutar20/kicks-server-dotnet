namespace Application.Abstractions.FileUploader;

public sealed record FileUploadResult(
    Guid Id,
    Uri Uri,
    string FileName,
    long Size,
    string ContentType
);
