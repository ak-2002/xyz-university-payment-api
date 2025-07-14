namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class StudentFeeBalance
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int FeeStructureItemId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DueDate { get; set; }
        public FeeBalanceStatus Status { get; set; } = FeeBalanceStatus.Outstanding;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public FeeStructureItem? FeeStructureItem { get; set; }
    }

    public enum FeeBalanceStatus
    {
        Outstanding,  // No payment made
        Partial,      // Partial payment made
        Paid,         // Fully paid
        Overdue       // Past due date
    }
} 