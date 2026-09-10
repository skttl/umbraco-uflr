using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using uFlr.Services;

namespace uFlr.NotificationHandlers;

public class MediaNotifications
    : NotificationHandlerBase,
        INotificationAsyncHandler<MediaSavingNotification>,
        INotificationAsyncHandler<MediaDeletedNotification>
{
    public MediaNotifications(
        ILogger<MediaNotifications> logger,
        IDataTypeService dataTypeService,
        ICloudflareService cloudflareService,
        MediaFileManager mediaFileManager
    )
        : base(logger, dataTypeService, cloudflareService, mediaFileManager) { }

    public async Task HandleAsync(
        MediaSavingNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.SavedEntities.Select(TrySyncUploadFilesToCloudflare));

    public async Task HandleAsync(
        MediaDeletedNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedEntities.Select(TryDeleteSyncedUploadFilesFromCloudflare));
}
