namespace xyz_university_payment_api.Models
{
    public class Student
    {
        public int Id { get; set; }
        public required string StudentNumber { get; set; }
        public required string FullName { get; set; }
        public required string Program { get; set; }
        public bool IsActive { get; set; }
    }
}