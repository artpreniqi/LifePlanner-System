using System;
using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models
{
    public class LifeGoal
    {
        public int Id { get; set; }

        [Required]
        public string? GoalName { get; set; }

        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        public int ProgressPercentage { get; set; } // 0 - 100%

        public string Category { get; set; } = "General"; // Default value

        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }
    }
}