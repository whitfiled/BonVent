using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BonVent.Contracts.Rooms;
using Domain.Entities.RoomEntity;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

internal sealed class AzureBlobRoomImageService(HotelDbContext db, IOptions<BlobStorageOptions> options) : IRoomImageService
{
    private BlobServiceClient BlobServiceClient =>
        string.IsNullOrWhiteSpace(options.Value.ConnectionString)
            ? throw new InvalidOperationException("Blob storage is not configured.")
            : new BlobServiceClient(options.Value.ConnectionString);

    public async Task<RoomImageResponse> AddAsync(Guid roomId, RoomImageUpload upload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(upload);

        var room = await db.Rooms.Include(item => item.Images).SingleOrDefaultAsync(item => item.Id == roomId, cancellationToken)
            ?? throw new NotFoundException("Room not found.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/svg" };

        if (!allowedTypes.Contains(upload.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only JPEG, PNG, SVG, and WebP images are supported.", nameof(upload));
        }

        if (upload.Content.Length is <= 0 or > 5_000_000)
        {
            throw new ArgumentException("The image must be between 1 byte and 5 MB.", nameof(upload));
        }

        var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();

        var blobName = $"{roomId:N}/{Guid.NewGuid():N}{extension}";
        var container = BlobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(upload.Content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = upload.ContentType }
        }, cancellationToken);

        var image = new RoomImage(
            roomId,
            blobName,
            upload.ContentType,
            upload.AltText,
            room.Images.Count == 0,
            room.Images.Count);

        db.Set<RoomImage>().Add(image);
        await db.SaveChangesAsync(cancellationToken);

        return new RoomImageResponse(
            image.Id,
            $"/api/v1/rooms/{roomId}/images/{image.Id}",
            image.AltText,
            image.IsPrimary,
            image.SortOrder);
    }

    public async Task<StoredRoomImage> GetAsync(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await db.Set<RoomImage>().AsNoTracking().SingleOrDefaultAsync(item => item.Id == imageId && item.RoomId == roomId, cancellationToken)
            ?? throw new NotFoundException("Room image not found.");

        var container = BlobServiceClient.GetBlobContainerClient(options.Value.ContainerName);

        var blob = container.GetBlobClient(image.BlobName);

        var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);

        return new StoredRoomImage(download.Value.Content, image.ContentType);
    }
}
