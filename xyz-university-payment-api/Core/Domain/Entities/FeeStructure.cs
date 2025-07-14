namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class FeeStructure
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., "Standard 2025", "Premium 2025"
        public string Description { get; set; } = string.Empty;
        public required string AcademicYear { get; set; }
        public required string Semester { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public List<FeeStructureItem> FeeStructureItems { get; set; } = new List<FeeStructureItem>();
        public List<StudentFeeAssignment> StudentFeeAssignments { get; set; } = new List<StudentFeeAssignment>();
    }
} 