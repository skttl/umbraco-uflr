using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uFlr.Services;

namespace uFlr.NotificationHandlers;

public class MemberNotifications
    : NotificationHandlerBase,
        INotificationAsyncHandler<MemberDeletedNotification>,
        INotificationAsyncHandler<MemberSavingNotification>
{
    public MemberNotifications(
        ILogger<MemberNotifications> logger,
        IDataTypeService dataTypeService,
        ICloudflareService cloudflareService,
        MediaFileManager mediaFileManager
    )
        : base(logger, dataTypeService, cloudflareService, mediaFileManager) { }

    public Task HandleAsync(
        MemberDeletedNotification notification,
        CancellationToken cancellationToken
    ) => Task.WhenAll(notification.DeletedEntities.Select(TryDeleteSyncedUploadFilesFromCloudflare));

    public async Task HandleAsync(
        MemberSavingNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.SavedEntities.Select(TrySyncUploadFilesToCloudflare));
}
