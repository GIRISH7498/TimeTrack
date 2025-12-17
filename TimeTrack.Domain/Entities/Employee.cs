namespace TimeTrack.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EmployeeCode { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public DateTime? JoiningDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
