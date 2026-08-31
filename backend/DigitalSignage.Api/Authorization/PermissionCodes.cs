namespace DigitalSignage.Api.Authorization;

public static class PermissionCodes
{
    public const string DashboardView = "dashboard.view";

    public const string DevicesView = "devices.view";
    public const string DevicesManage = "devices.manage";

    public const string MediaView = "media.view";
    public const string MediaManage = "media.manage";

    public const string PlaylistsView = "playlists.view";
    public const string PlaylistsManage = "playlists.manage";

    public const string AssignmentsView = "assignments.view";
    public const string AssignmentsManage = "assignments.manage";

    public const string ReferencesView = "references.view";
    public const string ReferencesManage = "references.manage";

    public const string SyncLogsView = "sync_logs.view";

    public const string UsersManage = "users.manage";

    public static readonly string[] All =
    [
        DashboardView,

        DevicesView,
        DevicesManage,

        MediaView,
        MediaManage,

        PlaylistsView,
        PlaylistsManage,

        AssignmentsView,
        AssignmentsManage,

        ReferencesView,
        ReferencesManage,

        SyncLogsView,

        UsersManage
    ];
}