namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class PaymentPlan
    {
        public int Id { get; set; }
        public required string StudentNumber { get; set; }
        public int StudentBalanceId { get; set; }
        public string PlanType { get; set; } = "Standard"; // Standard, Installment, Custom
        public int TotalInstallments { get; set; }
        public int CompletedInstallments { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Overdue, Cancelled
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        
        // Navigation properties
        public Student? Student { get; set; }
        public StudentBalance? StudentBalance { get; set; }
        public List<PaymentNotification> Payments { get; set; } = new List<PaymentNotification>();
    }
} 