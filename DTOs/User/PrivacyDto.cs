namespace TheDriveAPI.DTOs.User
{
    public class PrivacyDto
    {
        public bool? TwoFactorEnabled { get; set; }
        public bool? ShareEmail { get; set; }
        public bool? ShareActivity { get; set; }
    }
}
