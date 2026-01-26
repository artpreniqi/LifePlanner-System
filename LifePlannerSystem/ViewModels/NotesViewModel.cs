using LifePlanner.Models; 
using System.Collections.Generic;

namespace LifePlanner.ViewModels
{
    public class NotesViewModel
    {
        public List<PersonalNote> AllNotes { get; set; }
        public PersonalNote CurrentNote { get; set; }

        public bool IsEditMode => CurrentNote != null && CurrentNote.Id > 0;
    }
}