using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using CadentManagement.Authorization;

namespace CadentManagement.Web.Areas.App.Startup;

public class AppNavigationProvider : NavigationProvider
{
    public const string MenuName = "App";

    public override void SetNavigation(INavigationProviderContext context)
    {
        var menu = context.Manager.Menus[MenuName] = new MenuDefinition(MenuName, new FixedLocalizableString("Main Menu"));

        menu
            .AddItem(new MenuItemDefinition(
                    AppPageNames.Common.Saas,
                    L("Saas"),
                    icon: "flaticon-users"
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Host.Tenants,
                        L("Tenants"),
                        url: "App/Tenants",
                        icon: "flaticon-list-3",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tenants)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Host.Editions,
                        L("Editions"),
                        url: "App/Editions",
                        icon: "flaticon-app",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Editions)
                    )
                )
            ).AddItem(new MenuItemDefinition(
                    AppPageNames.Tenant.TimeTracking,
                    L("TimeTracking"),
                    icon: "flaticon-time",
                    permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeTracking)
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TimeTracking,
                        L("Projects"),
                        url: "App/TimeTracking",
                        icon: "flaticon-folder",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeTracking)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TimeTrackingMyWeek,
                        L("MyWeek"),
                        url: "App/TimeTracking/MyWeek",
                        icon: "flaticon-calendar-1",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeTracking_TimeEntries)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TimeTrackingReports,
                        L("TimeTrackingReports"),
                        url: "App/TimeTracking/Reports",
                        icon: "flaticon-analytics",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeTracking_Reports)
                    )
                )
            ).AddItem(new MenuItemDefinition(
                    AppPageNames.Tenant.Tasks,
                    L("ToDos"),
                    icon: "flaticon-layers",
                    permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tasks)
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TasksStatusBoard,
                        L("ToDoStatusBoard"),
                        url: "App/Tasks",
                        icon: "flaticon-list-1",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tasks)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TasksPlanner,
                        L("ToDoDateBoard"),
                        url: "App/Tasks/Planner",
                        icon: "flaticon-calendar-1",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tasks)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.TodoStatuses,
                        L("ToDoStatuses"),
                        url: "App/TodoStatuses",
                        icon: "flaticon2-gear",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tasks)
                    )
                )
            ).AddItem(new MenuItemDefinition(
                    AppPageNames.Common.Administration,
                    L("Administration"),
                    icon: "flaticon-interface-8"
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.OrganizationUnits,
                        L("OrganizationUnits"),
                        url: "App/OrganizationUnits",
                        icon: "flaticon-map",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_OrganizationUnits)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Roles,
                        L("Roles"),
                        url: "App/Roles",
                        icon: "flaticon-suitcase",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Roles)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Users,
                        L("Users"),
                        url: "App/Users",
                        icon: "flaticon-users",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Users)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Languages,
                        L("Languages"),
                        url: "App/Languages",
                        icon: "flaticon-tabs",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Languages)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.AuditLogs,
                        L("AuditLogs"),
                        url: "App/AuditLogs",
                        icon: "flaticon-folder-1",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_AuditLogs)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Host.Maintenance,
                        L("Maintenance"),
                        url: "App/Maintenance",
                        icon: "flaticon-lock",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Host_Maintenance)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Host.HealthCheck,
                        L("HealthCheck"),
                        url: "App/HealthCheckUI",
                        icon: "flaticon2-cardiogram",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Host_HealthCheck)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.SubscriptionManagement,
                        L("Subscription"),
                        url: "App/SubscriptionManagement",
                        icon: "flaticon-refresh",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Tenant_SubscriptionManagement)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.UiCustomization,
                        L("VisualSettings"),
                        url: "App/UiCustomization",
                        icon: "flaticon-medical",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_UiCustomization)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.WebhookSubscriptions,
                        L("WebhookSubscriptions"),
                        url: "App/WebhookSubscription",
                        icon: "flaticon2-world",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_WebhookSubscription)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Common.DynamicProperties,
                        L("DynamicProperties"),
                        url: "App/DynamicProperty",
                        icon: "flaticon-interface-8",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_DynamicProperties)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Notifications,
                        L("Notifications"),
                        icon: "flaticon-alarm"
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Notifications_Inbox,
                            L("Inbox"),
                            url: "App/Notifications",
                            icon: "flaticon-mail-1"
                        )
                    )
                    .AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Notifications_MassNotifications,
                            L("MassNotifications"),
                            url: "App/Notifications/MassNotifications",
                            icon: "flaticon-paper-plane",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_MassNotification)
                        )
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Host.RateLimiting,
                        L("RateLimiting"),
                        url: "App/RateLimiting",
                        icon: "flaticon-security",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_RateLimiting)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Host.Settings,
                        L("Settings"),
                        url: "App/HostSettings",
                        icon: "flaticon-settings",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Host_Settings)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.Settings,
                        L("Settings"),
                        url: "App/Settings",
                        icon: "flaticon-settings",
                        permissionDependency: new SimplePermissionDependency(AppPermissions
                            .Pages_Administration_Tenant_Settings)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppPageNames.Common.ActiveSessions,
                        L("ActiveSessions"),
                        url: "App/ActiveSessions",
                        icon: "flaticon-security",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_ActiveSessions)
                    )
                )
            );
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, CadentManagementConsts.LocalizationSourceName);
    }
}
