using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uFlr.Models;
using uFlr.Services;

namespace uFlr.NotificationHandlers;

public abstract class NotificationHandlerBase
{
    private readonly ILogger<NotificationHandlerBase> _logger;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICloudflareService _cloudflareService;
    private readonly MediaFileManager _mediaFileManager;

    public NotificationHandlerBase(
        ILogger<NotificationHandlerBase> logger,
        IDataTypeService dataTypeService,
        ICloudflareService cloudflareService,
        MediaFileManager mediaFileManager)
    {
        _logger = logger;
        _dataTypeService = dataTypeService;
        _cloudflareService = cloudflareService;
        _mediaFileManager = mediaFileManager;
    }

    public async Task<bool> TryDeleteSyncedUploadFilesFromCloudflare(IContentBase node)
    {
        try
        {
            return await DeleteSyncedUploadFilesFromCloudflare(node);
        }
        catch
        {
            _logger.LogError(
                "An error occurred while deleting synced upload files from Cloudflare for content with ID {ContentId}",
                node.Id
            );
            return false;
        }
    }

    public async Task<bool> TrySyncUploadFilesToCloudflare(IContentBase node)
    {
        try
        {
            return await SyncUploadFilesToCloudflare(node);
        }
        catch
        {
            _logger.LogError(
                "An error occurred while syncing upload files to Cloudflare for content with ID {ContentId}",
                node.Id
            );
            return false;
        }
    }

    /// <summary>
    /// Syncs upload files to Cloudflare when the content has uFlr sync properties.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>boolean indicating if any changes were made</returns>
    public async Task<bool> SyncUploadFilesToCloudflare(IContentBase node)
    {
        var isUpdated = false;

        var cloudflareSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var cloudflareSyncProperty in cloudflareSyncProperties)
        {
            var dataType = await _dataTypeService.GetAsync(
                cloudflareSyncProperty.PropertyType.DataTypeKey
            );

            if (
                dataType is null
                || dataType.ConfigurationData.TryGetValue(
                    Constants.UploadPropertyAlias,
                    out var value
                )
                    is false
                || value is not string uploadPropertyAlias
                || string.IsNullOrWhiteSpace(uploadPropertyAlias)
            )
            {
                continue;
            }

            var existingStringValue = node.GetValue<string>(cloudflareSyncProperty.Alias);

            var existingValue =
                existingStringValue.IsNullOrWhiteSpace() is false
                && existingStringValue.StartsWith("{")
                    ? JsonSerializer.Deserialize<CloudflareValue>(existingStringValue)
                    : null;

            var canContinue =
                node.IsPropertyDirty(uploadPropertyAlias)
                || existingValue?.Src != node.GetValue<string>(uploadPropertyAlias);

            if (canContinue == false)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(existingValue?.CloudflareAssetId) is false)
            {
                await _cloudflareService.DeleteAsset(existingValue.CloudflareAssetId);
                node.SetValue(cloudflareSyncProperty.Alias, null);
                isUpdated = true;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var fileStream = _mediaFileManager.GetFile(node, out var _, uploadPropertyAlias);
                fileStream.CopyTo(ms);
                var byteArray = ms.ToArray();

                if (byteArray is not null && byteArray.Length > 0)
                {
                    var asset = await _cloudflareService.CreateAsset(
                        byteArray,
                        node.Name,
                        node.CreatorId.ToString(),
                        node.GetUdi().ToString().EnsureEndsWith($"/{uploadPropertyAlias}")
                    );

                    node.SetValue(
                        cloudflareSyncProperty.Alias,
                        JsonSerializer.Serialize(
                            new CloudflareValue()
                            {
                                CloudflareAssetId = asset.AssetId,
                                PlaybackUrl = asset.Output?.PlaybackUrl,
                                Src = node.GetValue<string>(uploadPropertyAlias),
                            }
                        )
                    );

                    isUpdated = true;
                }
            }
        }

        return isUpdated;
    }

    /// <summary>
    /// Deletes the files from Cloudflare if the content has any uFlr sync properties.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>boolean indicating if any changes were made</returns>
    public async Task<bool> DeleteSyncedUploadFilesFromCloudflare(IContentBase node)
    {
        var isUpdated = false;

        var cloudflareSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var cloudflareSyncProperty in cloudflareSyncProperties)
        {
            var dataType = await _dataTypeService.GetAsync(
                cloudflareSyncProperty.PropertyType.DataTypeKey
            );

            var existingStringValue = node.GetValue<string>(cloudflareSyncProperty.Alias);

            var existingValue =
                existingStringValue.IsNullOrWhiteSpace() is false
                && existingStringValue.StartsWith("{")
                    ? JsonSerializer.Deserialize<CloudflareValue>(existingStringValue)
                    : null;

            if (existingValue is null || existingValue.CloudflareAssetId.IsNullOrWhiteSpace())
            {
                continue;
            }

            await _cloudflareService.DeleteAsset(existingValue.CloudflareAssetId);
            node.SetValue(cloudflareSyncProperty.Alias, null);
            isUpdated = true;
        }

        return isUpdated;
    }

    public static bool ResetCloudflareValuesWithoutDeleting(IContentBase node)
    {
        var isUpdated = false;

        var cloudflareSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var cloudflareSyncProperty in cloudflareSyncProperties)
        {
            node.SetValue(cloudflareSyncProperty.Alias, null);
            isUpdated = isUpdated || node.IsPropertyDirty(cloudflareSyncProperty.Alias);
        }

        return isUpdated;
    }
}
