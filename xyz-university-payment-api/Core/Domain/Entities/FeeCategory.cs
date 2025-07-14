namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class FeeCategory
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., "Tuition", "Lunch", "Uniform", "Laboratory"
        public string Description { get; set; } = string.Empty;
        public FeeCategoryType Type { get; set; } = FeeCategoryType.Standard;
        public FeeFrequency Frequency { get; set; } = FeeFrequency.OneTime;
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public List<FeeStructureItem> FeeStructureItems { get; set; } = new List<FeeStructureItem>();
    }

    public enum FeeCategoryType
    {
        Standard,   // Regular fees like tuition, lunch
        Additional  // Optional fees like trips, sports
    }

    public enum FeeFrequency
    {
        OneTime,    // Single payment
        Recurring   // Monthly, quarterly, etc.
    }
} 