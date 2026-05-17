using Application.Abstractions.FileUploader;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using File = Application.Abstractions.FileUploader.File;

namespace Infrastructure.Services;

internal sealed class FileUploaderService(Config config) : IFileUploader
{
    private readonly Cloudinary _cloudinary = new(
        new Account(config.Cloudinary.CloudName, config.Cloudinary.ApiKey, config.Cloudinary.Secret)
    );

    public async Task DeleteFilesAsync(List<Guid> ids, CancellationToken ct = default) =>
        await _cloudinary.DeleteResourcesAsync(
            new DelResParams()
            {
                PublicIds = ids.Select(id => $"{config.Cloudinary.Folder}/{id}").ToList(),
            },
            ct
        );

    public async Task DeleteFileAsync(Guid id) =>
        await _cloudinary.DestroyAsync(new DeletionParams($"{config.Cloudinary.Folder}/{id}"));

    public async Task<FileUploadResult> UploadAsync(File file, CancellationToken ct = default)
    {
        var publicId = Guid.NewGuid();
        var uploadParams = new AutoUploadParams()
        {
            File = new FileDescription(file.FileName.Value, file.Content),
            Folder = config.Cloudinary.Folder,
            UseFilename = true,
            PublicId = publicId.ToString(),
            FilenameOverride = file.FileName.Value,
            Format = Path.GetExtension(file.FileName.Value)[1..],
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

        return new FileUploadResult(
            publicId,
            uploadResult.SecureUrl,
            uploadResult.OriginalFilename,
            uploadResult.Bytes,
            uploadResult.Type
        );
    }
}
