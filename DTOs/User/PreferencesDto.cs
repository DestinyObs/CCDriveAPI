namespace TheDriveAPI.DTOs.User
{
    public class PreferencesDto
    {
        public bool? DarkMode { get; set; }
        public NotificationSettingsDto? Notifications { get; set; }
    }
    public class NotificationSettingsDto
    {
        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public bool? FileSharing { get; set; }
        public bool? Storage { get; set; }
    }
}
