namespace TheDriveAPI.DTOs.Pricing
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long StorageLimit { get; set; }
        public string Features { get; set; } = string.Empty;
    }
}
