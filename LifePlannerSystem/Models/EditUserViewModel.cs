using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }

        // Change role
        public bool IsAdmin { get; set; } 
    }
}