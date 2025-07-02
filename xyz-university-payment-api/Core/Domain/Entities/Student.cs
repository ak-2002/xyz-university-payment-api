namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public required string StudentNumber { get; set; }
        public required string FullName { get; set; }
        public required string Program { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}