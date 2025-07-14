namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class StudentFeeAssignment
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int FeeStructureId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedBy { get; set; } = string.Empty; // Admin username
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public FeeStructure? FeeStructure { get; set; }
    }
} 