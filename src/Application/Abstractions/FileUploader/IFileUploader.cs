namespace Application.Abstractions.FileUploader;

public interface IFileUploader
{
    Task<Result<FileUploadResult>> UploadFileAsync(FileData file, CancellationToken ct = default);

    Task<Result<IEnumerable<FileUploadResult>>> UploadFilesAsync(
        IEnumerable<FileData> files,
        CancellationToken ct = default
    );

    Task DeleteFilesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task DeleteFileAsync(Guid id);
}
