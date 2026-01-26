using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } // Select direct if user is Admin
    }
}