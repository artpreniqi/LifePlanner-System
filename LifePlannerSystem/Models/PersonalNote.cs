using System;
using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models
{
    public class PersonalNote
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Category { get; set; } // Work, Personal, Ideas

        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }
    }
}