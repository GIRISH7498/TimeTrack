using Microsoft.AspNetCore.Identity;
using TimeTrack.Domain.Entities;

namespace TimeTrack.Infrastructure.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation to Employee (one-to-one)
        public Employee? Employee { get; set; }
    }
}
