using Microsoft.AspNetCore.Identity;

namespace LifePlanner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}