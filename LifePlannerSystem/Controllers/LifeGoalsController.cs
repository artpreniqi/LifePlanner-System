using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Data;
using LifePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class LifeGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LifeGoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LifeGoals
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var goals = _context.LifeGoals.Include(g => g.Owner).AsQueryable();

            if (!isAdmin)
            {
                goals = goals.Where(g => g.OwnerId == userId);
            }

            return View(await goals.ToListAsync());
        }

        // GET: LifeGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var goal = await _context.LifeGoals
                .Include(g => g.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (goal == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.OwnerId != userId && !User.IsInRole("Admin")) return Forbid();

            return View(goal);
        }

        // GET: LifeGoals/Create
        public IActionResult Create(string? prefillDate)
        {
            var model = new LifeGoal();

            if (!string.IsNullOrEmpty(prefillDate))
            {
                // If it comes from calendar, parse it
                if (DateTime.TryParse(prefillDate, out DateTime parsedDate))
                {
                    model.TargetDate = parsedDate;
                }
            }
            else
            {
                // Default: Today Date
                model.TargetDate = DateTime.Now;
            }

            return View(model);
        }

        // POST: LifeGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LifeGoal goal)
        {
            goal.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Add(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        // GET: LifeGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var goal = await _context.LifeGoals.FindAsync(id);
            if (goal == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(goal);
        }

        // POST: LifeGoals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LifeGoal goal)
        {
            if (id != goal.Id) return NotFound();

            var originalGoal = await _context.LifeGoals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (originalGoal == null) return NotFound();
            
            goal.OwnerId = originalGoal.OwnerId; 
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Update(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        // GET: LifeGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var goal = await _context.LifeGoals.FindAsync(id);
            if (goal == null) return NotFound();
            return View(goal);
        }

        // POST: LifeGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var goal = await _context.LifeGoals.FindAsync(id);
            if (goal != null) { _context.LifeGoals.Remove(goal); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // POST: LifeGoals/UpdateProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int id, int newProgress)
        {
            var goal = await _context.LifeGoals.FindAsync(id);
            if (goal == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.OwnerId != userId && !User.IsInRole("Admin")) return Forbid();

            if (newProgress < 0) newProgress = 0;
            if (newProgress > 100) newProgress = 100;

            goal.ProgressPercentage = newProgress; 
            
            _context.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}