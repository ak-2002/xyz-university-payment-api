namespace xyz_university_payment_api.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public string Program { get; set; }
        public bool IsActive { get; set; }
    }
}