using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using uFlr.Services;

namespace uFlr.NotificationHandlers;

public class ContentNotifications
    : NotificationHandlerBase,
        INotificationHandler<ContentCopyingNotification>,
        INotificationAsyncHandler<ContentDeletedBlueprintNotification>,
        INotificationAsyncHandler<ContentDeletedNotification>,
        INotificationHandler<ContentSavedBlueprintNotification>,
        INotificationAsyncHandler<ContentSavingNotification>
{
    private readonly IContentService _contentService;

    public ContentNotifications(
        ILogger<ContentNotifications> logger,
        IContentService contentService,
        IDataTypeService dataTypeService,
        ICloudflareService cloudflareService,
        MediaFileManager mediaFileManager
    )
        : base(logger, dataTypeService, cloudflareService, mediaFileManager)
    {
        _contentService = contentService;
    }

    public void Handle(ContentCopyingNotification notification) =>
        ResetCloudflareValuesWithoutDeleting(notification.Copy);

    public async Task HandleAsync(
        ContentDeletedBlueprintNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedBlueprints.Select(TryDeleteSyncedUploadFilesFromCloudflare));

    public async Task HandleAsync(
        ContentDeletedNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedEntities.Select(TryDeleteSyncedUploadFilesFromCloudflare));

    public void Handle(ContentSavedBlueprintNotification notification)
    {
        if (ResetCloudflareValuesWithoutDeleting(notification.SavedBlueprint))
        {
            _contentService.SaveBlueprint(
                notification.SavedBlueprint,
                notification.CreatedFromContent
            );
        }
    }

    public async Task HandleAsync(
        ContentSavingNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.SavedEntities.Select(TrySyncUploadFilesToCloudflare));
}
