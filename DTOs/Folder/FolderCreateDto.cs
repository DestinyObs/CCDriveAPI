namespace TheDriveAPI.DTOs.Folder
{
    public class FolderCreateDto
    {
        public required string Name { get; set; }
        public int? ParentId { get; set; }
    }
}
