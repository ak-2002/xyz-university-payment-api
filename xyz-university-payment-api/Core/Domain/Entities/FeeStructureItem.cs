namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class FeeStructureItem
    {
        public int Id { get; set; }
        public int FeeStructureId { get; set; }
        public int FeeCategoryId { get; set; }
        public decimal Amount { get; set; }
        public bool IsRequired { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public FeeStructure? FeeStructure { get; set; }
        public FeeCategory? FeeCategory { get; set; }
        public List<StudentFeeBalance> StudentFeeBalances { get; set; } = new List<StudentFeeBalance>();
    }
} 