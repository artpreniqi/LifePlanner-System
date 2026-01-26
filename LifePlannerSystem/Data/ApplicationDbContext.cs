using LifePlanner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LifePlanner.Data
{
    // User Table inherits from IdentityDbContext 
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // DbSets for application entities
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<LifeGoal> LifeGoals { get; set; }
        public DbSet<PersonalNote> PersonalNotes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Audit log automatic changes tracking
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                
                if (entry.Entity is AuditLog) continue;

                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                
                var audit = new AuditLog
                {
                    UserId = userId,
                    Action = entry.State.ToString(),
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Entity ID changed. State: {entry.State}"
                };

                AuditLogs.Add(audit);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}