using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Data;
using LifePlanner.Models;
using LifePlanner.ViewModels;

namespace LifePlanner.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTasks = await _context.ToDoItems.CountAsync();
            ViewBag.TotalGoals = await _context.LifeGoals.CountAsync();
            ViewBag.TotalNotes = await _context.PersonalNotes.CountAsync();

            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userTasks = await _context.ToDoItems.Where(t => t.OwnerId == id).OrderBy(t => t.DueDate).ToListAsync();
            var userGoals = await _context.LifeGoals.Where(g => g.OwnerId == id).ToListAsync();
            var userNotes = await _context.PersonalNotes.Where(n => n.OwnerId == id).OrderByDescending(n => n.CreatedAt).ToListAsync();

            ViewBag.TargetUser = user;
            ViewBag.Tasks = userTasks;
            ViewBag.Goals = userGoals;
            ViewBag.Notes = userNotes;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserTask(int id, string returnUserId)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task != null) { _context.ToDoItems.Remove(task); await _context.SaveChangesAsync(); }
            return RedirectToAction("UserDetails", new { id = returnUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserGoal(int id, string returnUserId)
        {
            var goal = await _context.LifeGoals.FindAsync(id);
            if (goal != null) { _context.LifeGoals.Remove(goal); await _context.SaveChangesAsync(); }
            return RedirectToAction("UserDetails", new { id = returnUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserNote(int id, string returnUserId)
        {
            var note = await _context.PersonalNotes.FindAsync(id);
            if (note != null) { _context.PersonalNotes.Remove(note); await _context.SaveChangesAsync(); }
            return RedirectToAction("UserDetails", new { id = returnUserId });
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id, Email = user.Email, UserName = user.UserName, FullName = user.FullName, IsAdmin = userRoles.Contains("Admin")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();
            user.Email = model.Email; user.UserName = model.UserName; user.FullName = model.FullName;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var isCurrentlyAdmin = currentRoles.Contains("Admin");
                if (model.IsAdmin && !isCurrentlyAdmin) await _userManager.AddToRoleAsync(user, "Admin");
                else if (!model.IsAdmin && isCurrentlyAdmin) {
                    if (User.Identity.Name != user.UserName) await _userManager.RemoveFromRoleAsync(user, "Admin");
                }
                return RedirectToAction("Index"); 
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var tasks = _context.ToDoItems.Where(t => t.OwnerId == id); _context.ToDoItems.RemoveRange(tasks);
                var goals = _context.LifeGoals.Where(g => g.OwnerId == id); _context.LifeGoals.RemoveRange(goals);
                var notes = _context.PersonalNotes.Where(n => n.OwnerId == id); _context.PersonalNotes.RemoveRange(notes);
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        public IActionResult CreateUser() => View();

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.IsAdmin) await _userManager.AddToRoleAsync(user, "Admin");
                    else await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTasks = await _context.ToDoItems.CountAsync();
            ViewBag.TotalGoals = await _context.LifeGoals.CountAsync();
            ViewBag.TotalNotes = await _context.PersonalNotes.CountAsync();

            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
}