namespace TheDriveAPI.DTOs.User
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? PlanId { get; set; }
    }
}
