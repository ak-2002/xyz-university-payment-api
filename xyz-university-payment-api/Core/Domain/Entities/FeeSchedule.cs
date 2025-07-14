namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class FeeSchedule
    {
        public int Id { get; set; }
        public required string Semester { get; set; } // e.g., "Summer 2025", "Fall 2025"
        public required string AcademicYear { get; set; } // e.g., "2024-2025"
        public required string Program { get; set; } // e.g., "Computer Science", "Business Administration"
        public decimal TuitionFee { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public decimal OtherFees { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
        
        // Navigation properties
        public List<StudentBalance> StudentBalances { get; set; } = new List<StudentBalance>();
    }
} 