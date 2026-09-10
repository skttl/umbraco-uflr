using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using uFlr.Configuration;
using uFlr.NotificationHandlers;
using uFlr.Services;

namespace uFlr.Composers;

public class CloudflareComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // load up the settings.
        var options = builder
            .Services.AddOptions<CloudflareSettings>()
            .Bind(builder.Config.GetSection(Constants.AppSettingsPath));

        options.ValidateDataAnnotations();

        builder.Services.AddHttpClient();
        builder.Services.AddScoped<ICloudflareService, CloudflareService>();

        builder.AddBackOfficeOpenApiDocument(
            Constants.Swagger.ApiName,
            document =>
                document
                    .WithTitle(Constants.Swagger.Title)
                    .WithUiTitle(Constants.Swagger.Title)
                    .WithBackOfficeAuthentication()
        );

        // media
        builder.AddNotificationAsyncHandler<MediaSavingNotification, MediaNotifications>();
        builder.AddNotificationAsyncHandler<MediaDeletedNotification, MediaNotifications>();

        // content
        builder.AddNotificationHandler<ContentCopyingNotification, ContentNotifications>();
        builder.AddNotificationAsyncHandler<
            ContentDeletedBlueprintNotification,
            ContentNotifications
        >();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentNotifications>();
        builder.AddNotificationHandler<ContentSavedBlueprintNotification, ContentNotifications>();
        builder.AddNotificationAsyncHandler<ContentSavingNotification, ContentNotifications>();

        // member
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, MemberNotifications>();
        builder.AddNotificationAsyncHandler<MemberSavingNotification, MemberNotifications>();

    }
}
