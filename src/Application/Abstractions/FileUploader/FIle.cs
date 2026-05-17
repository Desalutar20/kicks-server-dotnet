namespace Application.Abstractions.FileUploader;

public sealed record File(Stream Content, NonEmptyString FileName, NonEmptyString ContentType);
