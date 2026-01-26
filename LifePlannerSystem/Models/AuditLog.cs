using System;

namespace LifePlanner.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; } // Create, Read, Update, Delete
        public string? EntityName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}