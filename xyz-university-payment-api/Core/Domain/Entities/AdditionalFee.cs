namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class AdditionalFee
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., "Science Trip", "Sports Tournament"
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public FeeFrequency Frequency { get; set; } = FeeFrequency.OneTime;
        public FeeApplicability ApplicableTo { get; set; } = FeeApplicability.All;
        public string? ApplicablePrograms { get; set; } // JSON array of program names
        public string? ApplicableClasses { get; set; } // JSON array of class names
        public string? ApplicableStudents { get; set; } // JSON array of student numbers
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty; // Admin username
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public List<StudentAdditionalFee> StudentAdditionalFees { get; set; } = new List<StudentAdditionalFee>();
    }

    public enum FeeApplicability
    {
        All,        // All students
        Program,    // Specific programs
        Class,      // Specific classes
        Individual  // Specific students
    }
} 