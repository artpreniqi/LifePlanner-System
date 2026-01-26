using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Data;
using LifePlanner.Models;

namespace LifePlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TasksApi
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            
            var tasks = await _context.ToDoItems
                .Select(t => new 
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    OwnerName = t.Owner != null ? t.Owner.UserName : "Unknown" 
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/TasksApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.ToDoItems
                .Where(t => t.Id == id)
                .Select(t => new 
                {
                    Id = t.Id,
                    Title = t.Title,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }
    }
}