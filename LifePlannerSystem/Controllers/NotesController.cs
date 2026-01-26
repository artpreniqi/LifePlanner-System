using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Data;
using LifePlanner.Models;
using LifePlanner.ViewModels; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notes 
        public async Task<IActionResult> Index(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Get full list of notes for the user
            var allNotes = await _context.PersonalNotes
                .Where(n => n.OwnerId == userId)
                .OrderByDescending(n => n.CreatedAt) 
                .ToListAsync();

            // 2. Choose which note to display (new or existing)
            PersonalNote currentNote = new PersonalNote(); 

            if (id.HasValue)
            {
                var existingNote = allNotes.FirstOrDefault(n => n.Id == id);
                if (existingNote != null)
                {
                    currentNote = existingNote;
                }
            }

            // 3. ViemModel
            var viewModel = new NotesViewModel
            {
                AllNotes = allNotes,
                CurrentNote = currentNote
            };

            return View(viewModel);
        }

        // POST: Notes/Save 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PersonalNote note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (note.Id == 0)
            {
                // CREATE
                note.OwnerId = userId;
                note.CreatedAt = DateTime.Now;
                _context.Add(note);
            }
            else
            {
                // UPDATE 
                var existingNote = await _context.PersonalNotes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == note.Id);
                
                if (existingNote == null || existingNote.OwnerId != userId) 
                {
                    return Forbid();
                }

                note.OwnerId = userId;
                note.CreatedAt = existingNote.CreatedAt; 

                _context.Update(note);
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index), new { id = note.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _context.PersonalNotes.FindAsync(id);

            if (note != null && note.OwnerId == userId)
            {
                _context.PersonalNotes.Remove(note);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}