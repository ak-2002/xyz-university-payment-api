namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class StudentBalance
    {
        public int Id { get; set; }
        public required string StudentNumber { get; set; }
        public int FeeScheduleId { get; set; }
        public decimal TotalAmount { get; set; } // Total amount owed for this semester
        public decimal AmountPaid { get; set; } // Total amount paid for this semester
        public decimal OutstandingBalance { get; set; } // Calculated outstanding balance
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Outstanding"; // Outstanding, Paid, Overdue, Partial
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        
        // Navigation properties
        public Student? Student { get; set; }
        public FeeSchedule? FeeSchedule { get; set; }
        public List<PaymentNotification> Payments { get; set; } = new List<PaymentNotification>();
    }
} 