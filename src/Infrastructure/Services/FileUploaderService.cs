using System.Collections.Concurrent;
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

    public async Task DeleteFileAsync(Guid id)
    {
        var result = await _cloudinary.DestroyAsync(
            new DeletionParams($"{config.Cloudinary.Folder}/{id}")
        );

        if (result.Error is not null)
        {
            logger.LogWarning(
                "Cloudinary delete failed. FileId: {FileId}, Error: {Error}",
                id,
                result.Error.Message
            );
        }
    }

    public async Task DeleteFilesAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var result = await _cloudinary.DeleteResourcesAsync(
            new DelResParams()
            {
                PublicIds = ids.Select(id => $"{config.Cloudinary.Folder}/{id}").ToList(),
            },
            ct
        );

        if (result.Error is not null)
        {
            logger.LogError(
                "Cloudinary delete failed. Ids: {Ids}, Error: {Error}",
                ids,
                result.Error.Message
            );
        }
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        FileData file,
        CancellationToken ct = default
    )
    {
        var publicId = Guid.NewGuid();

        var uploadParams = new AutoUploadParams()
        {
            File = new FileDescription(file.FileName.FullName, file.Content),
            Folder = config.Cloudinary.Folder,
            UseFilename = true,
            PublicId = publicId.ToString(),
            FilenameOverride = file.FileName.FullName,
            Format = file.FileName.Extension[1..],
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);
        if (uploadResult.Error is not null)
        {
            return Domain.Abstractions.Error.Internal(
                $"Cloudinary upload failed: {uploadResult.Error.Message}"
            );
        }

        var fileUrlResult = FileUrl.Create(uploadResult.SecureUrl?.ToString() ?? "");
        if (fileUrlResult.IsFailure)
        {
            return fileUrlResult.Error;
        }

        return new FileUploadResult(
            publicId,
            fileUrlResult.Value,
            file.FileName,
            uploadResult.Bytes,
            file.ContentType
        );
    }

    public async Task<Result<IEnumerable<FileUploadResult>>> UploadFilesAsync(
        IEnumerable<FileData> files,
        CancellationToken ct = default
    )
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var results = new ConcurrentBag<Result<FileUploadResult>>();

        try
        {
            await Parallel.ForEachAsync(
                files,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = MaxParallelUploadsCount,
                    CancellationToken = cts.Token,
                },
                async (file, token) =>
                {
                    if (cts.IsCancellationRequested)
                        return;

                    var result = await UploadFileAsync(file, token);

                    results.Add(result);

                    if (result.IsFailure)
                    {
                        logger.LogError(
                            "File upload failed. File: {FileName}, Error: {Error}",
                            file.FileName.FullName,
                            result.Error.Description
                        );

                        cts.Cancel();
                    }
                }
            );

            return results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
        }
        catch (Exception ex)
        {
            var uploadedFiles = results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
            if (uploadedFiles.Count != 0)
            {
                await DeleteFilesAsync(uploadedFiles.Select(f => f.Id).ToList(), ct);
            }

            if (ex is not TaskCanceledException)
            {
                logger.LogError(ex, "Error while uploading files");
            }

            return Domain.Abstractions.Error.Failure("Failed to upload files");
        }
    }
}
