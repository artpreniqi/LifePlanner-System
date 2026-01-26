using LifePlanner.Data;
using LifePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Linq; 

namespace LifePlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // If user is logged in, show 
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                ViewBag.UserName = User.Identity?.Name ?? "Planner";

                // Dashboard Stats
                ViewBag.TotalTasks = await _context.ToDoItems.CountAsync(t => t.OwnerId == userId && !t.IsCompleted);
                ViewBag.CompletedTasks = await _context.ToDoItems.CountAsync(t => t.OwnerId == userId && t.IsCompleted);
               
                ViewBag.ActiveGoals = await _context.LifeGoals.CountAsync(g => g.OwnerId == userId && g.ProgressPercentage < 100);
               
                var upcomingTasks = await _context.ToDoItems
                    .Where(t => t.OwnerId == userId && !t.IsCompleted)
                    .OrderBy(t => t.DueDate)
                    .Take(3)
                    .ToListAsync();

                return View(upcomingTasks);
            }
          
            return View();
        }

        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get Tasks
            var tasks = await _context.ToDoItems
                .Where(t => t.OwnerId == userId && !t.IsCompleted)
                .Select(t => new {
                    id = t.Id,
                    title = "📝 " + t.Title,
                    start = t.DueDate.ToString("yyyy-MM-ddTHH:mm:ss"), // ISO Format
                    color = t.Priority == "High" ? "#dc3545" : (t.Priority == "Medium" ? "#ffc107" : "#198754"), 
                    url = "/ToDo/Details/" + t.Id
                }).ToListAsync();

            // Get Goals 
            var goals = await _context.LifeGoals
                .Where(g => g.OwnerId == userId && g.ProgressPercentage < 100)
                .Select(g => new {
                    id = g.Id,
                    title = "🏆 " + g.GoalName,
                    start = g.TargetDate.ToString("yyyy-MM-dd"),
                    color = "#0dcaf0", 
                    url = "/LifeGoals/Details/" + g.Id
                }).ToListAsync();

            var allEvents = tasks.Cast<object>().Concat(goals);

            return Json(allEvents);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}