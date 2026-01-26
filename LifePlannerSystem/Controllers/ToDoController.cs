using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Data;
using LifePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LifePlanner.Controllers
{
    [Authorize] // 5th Criterion: Only logged-in users can access ToDo functionalities
    public class ToDoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToDoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ToDo
        public async Task<IActionResult> Index(
            string sortOrder, 
            string currentFilter, 
            string searchString, 
            int? pageNumber,
            string filterPriority,   
            bool showCompleted = false) 
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["PrioritySortParm"] = sortOrder == "Priority" ? "priority_desc" : "Priority";

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;
            
            ViewData["CurrentPriority"] = filterPriority; 
            ViewData["ShowCompleted"] = showCompleted;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var tasks = _context.ToDoItems.Include(t => t.Owner).AsQueryable();

            if (!isAdmin)
            {
                tasks = tasks.Where(t => t.OwnerId == userId);
            }

            if (!showCompleted)
            {
                tasks = tasks.Where(t => !t.IsCompleted);
            }

            if (!string.IsNullOrEmpty(filterPriority))
            {
                tasks = tasks.Where(t => t.Priority == filterPriority);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                tasks = tasks.Where(s => 
                    (s.Title != null && s.Title.Contains(searchString)) || 
                    (s.Description != null && s.Description.Contains(searchString)));
            }

            // Sorting
            switch (sortOrder)
            {
                case "title_desc": tasks = tasks.OrderByDescending(s => s.Title); break;
                case "Date": tasks = tasks.OrderBy(s => s.DueDate); break;
                case "date_desc": tasks = tasks.OrderByDescending(s => s.DueDate); break;
                case "Priority": tasks = tasks.OrderBy(s => s.Priority); break;
                case "priority_desc": tasks = tasks.OrderByDescending(s => s.Priority); break;
                default: tasks = tasks.OrderBy(s => s.Title); break;
            }

            int pageSize = 5;
            return View(await PaginatedList<ToDoItem>.CreateAsync(tasks.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: ToDo/History
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var tasks = _context.ToDoItems.Include(t => t.Owner).AsQueryable();

            // Admin can see all, users only their own
            if (!isAdmin)
            {
                tasks = tasks.Where(t => t.OwnerId == userId);
            }

            // Get only completed tasks
            tasks = tasks.Where(t => t.IsCompleted);

            // Ranking by DueDate descending
            tasks = tasks.OrderByDescending(t => t.DueDate);

            return View(await tasks.ToListAsync());
        }

        // GET: ToDo/Create
        public IActionResult Create(string? prefillDate)
        {
            var model = new ToDoItem();
            if (!string.IsNullOrEmpty(prefillDate))
            {
                // If it comes from calendar with time
                if(DateTime.TryParse(prefillDate, out DateTime dt))
                {
                    model.DueDate = dt;
                }
            }
            else
            {
                // Default: Tomorrow
                model.DueDate = DateTime.Now.AddDays(1);
            }
            
            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,DueDate,Priority")] ToDoItem toDoItem)
        {
            // Set the OwnerId to the current user
            toDoItem.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            toDoItem.IsCompleted = false;

            // Remove Owner from validation
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Add(toDoItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toDoItem);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var toDoItem = await _context.ToDoItems
                .FirstOrDefaultAsync(m => m.Id == id);

            if (toDoItem == null) return NotFound();

            return View(toDoItem);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toDoItem = await _context.ToDoItems.FindAsync(id);
            if (toDoItem != null)
            {
                _context.ToDoItems.Remove(toDoItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var toDoItem = await _context.ToDoItems.FindAsync(id);
            if (toDoItem == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (toDoItem.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Stop unauthorized access
            }

            return View(toDoItem);
        }

        // POST: ToDo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ToDoItem toDoItem)
        {
            if (id != toDoItem.Id) return NotFound();

            // Get the original version to preserve OwnerId
            var originalItem = await _context.ToDoItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (originalItem == null) return NotFound();

            // Security check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalItem.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Force OwnerId to remain unchanged
            toDoItem.OwnerId = originalItem.OwnerId; 

            // Remove validation for Owner
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(toDoItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ToDoItems.Any(e => e.Id == toDoItem.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(toDoItem);
        }

        // GET: ToDo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoItem = await _context.ToDoItems
                .Include(t => t.Owner) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (toDoItem == null)
            {
                return NotFound();
            }

            // Security check
            // If the user is not the owner and not an admin, forbid access
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (toDoItem.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(toDoItem);
        }

        // POST: ToDo/MarkCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var toDoItem = await _context.ToDoItems.FindAsync(id);
            
            if (toDoItem == null)
            {
                return NotFound();
            }

            // Security: Admin or Owner only
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (toDoItem.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Set as completed
            toDoItem.IsCompleted = true;
            _context.Update(toDoItem);
            await _context.SaveChangesAsync();

            // Notify user of success
            TempData["SuccessMessage"] = "Task moved to History!";

            return RedirectToAction(nameof(Index));
        }

    }
}