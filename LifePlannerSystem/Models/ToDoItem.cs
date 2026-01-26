using System;
using System.ComponentModel.DataAnnotations;

namespace LifePlanner.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }

        [Required]
        public string? Priority { get; set; } // Low, Medium, High


        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }
    }
}