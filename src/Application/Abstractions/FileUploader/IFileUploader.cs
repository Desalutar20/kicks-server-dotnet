namespace Application.Abstractions.FileUploader;

public interface IFileUploader
{
    Task<FileUploadResult> UploadAsync(File file, CancellationToken ct = default);
    Task DeleteFilesAsync(List<Guid> ids, CancellationToken ct = default);
    Task DeleteFileAsync(Guid id);
}
