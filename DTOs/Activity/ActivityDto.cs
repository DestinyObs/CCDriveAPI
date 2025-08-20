namespace TheDriveAPI.DTOs.Activity
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? FileId { get; set; }
        public string Details { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
