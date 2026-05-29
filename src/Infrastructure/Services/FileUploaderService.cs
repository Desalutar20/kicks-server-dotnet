using Application.Abstractions.FileUploader;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Shared.FileContent;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

internal sealed class FileUploaderService(Config config, ILogger<FileUploaderService> logger)
    : IFileUploader
{
    private const int MaxParallelUploadsCount = 5;

    private readonly Cloudinary _cloudinary = new(
        new Account(config.Cloudinary.CloudName, config.Cloudinary.ApiKey, config.Cloudinary.Secret)
    );

    public async Task DeleteFileAsync(Guid id) =>
        await _cloudinary.DestroyAsync(new DeletionParams($"{config.Cloudinary.Folder}/{id}"));

    public async Task DeleteFilesAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        await _cloudinary.DeleteResourcesAsync(
            new DelResParams()
            {
                PublicIds = ids.Select(id => $"{config.Cloudinary.Folder}/{id}").ToList(),
            },
            ct
        );

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        FileData file,
        CancellationToken ct = default
    )
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

        var fileUrlResult = FileUrl.Create(uploadResult.SecureUrl.ToString());
        if (fileUrlResult.IsFailure)
        {
            return fileUrlResult.Error;
        }

        var fileNameResult = FileName.Create(uploadResult.OriginalFilename);
        if (fileNameResult.IsFailure)
        {
            return fileNameResult.Error;
        }

        var contentTypeResult = NonEmptyString.Create(
            uploadResult.Type,
            "contentType",
            "Content type"
        );
        if (contentTypeResult.IsFailure)
        {
            return contentTypeResult.Error;
        }

        return new FileUploadResult(
            publicId,
            fileUrlResult.Value,
            fileNameResult.Value,
            uploadResult.Bytes,
            contentTypeResult.Value
        );
    }

    public async Task<Result<IEnumerable<FileUploadResult>>> UploadFilesAsync(
        IEnumerable<FileData> files,
        CancellationToken ct = default
    )
    {
        using var semaphore = new SemaphoreSlim(MaxParallelUploadsCount);

        var tasks = files
            .Select(async file =>
            {
                await semaphore.WaitAsync(ct);

                try
                {
                    return await UploadFileAsync(file, ct);
                }
                finally
                {
                    semaphore.Release();
                }
            })
            .ToList();

        try
        {
            var results = await Task.WhenAll(tasks);

            var failedResult = results.FirstOrDefault(r => r.IsFailure);
            if (failedResult is null)
                return results.Select(r => r.Value).ToList();

            var uploadedFiles = results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
            if (uploadedFiles.Count != 0)
            {
                await DeleteFilesAsync(uploadedFiles.Select(f => f.Id), ct);
            }

            return failedResult.Error;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while uploading files");

            var uploadedFiles = tasks
                .Where(t => t.IsCompletedSuccessfully)
                .Select(t => t.Result)
                .Where(r => r.IsSuccess)
                .Select(r => r.Value)
                .ToList();

            if (uploadedFiles.Count != 0)
            {
                await DeleteFilesAsync(uploadedFiles.Select(f => f.Id), ct);
            }

            return Domain.Abstractions.Error.Internal("Failed to upload files");
        }
    }
}
