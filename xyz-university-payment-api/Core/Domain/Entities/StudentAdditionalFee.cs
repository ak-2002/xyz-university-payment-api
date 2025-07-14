namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class StudentAdditionalFee
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int AdditionalFeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public FeeBalanceStatus Status { get; set; } = FeeBalanceStatus.Outstanding;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public AdditionalFee? AdditionalFee { get; set; }
    }
} 