using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Entities
{
    // It's a running history of every important action in the system. Every important action in the system is recorded here.
    // Who did what, when. This is a real pattern used in every enterprise system.

    // Why AuditLog? In real enterprise systems, especially healthcare and finance you legally need to know who changed what and when.
    // If a patient complains "my appointment was cancelled without my knowledge," you can check the audit log to see exactly who cancelled it and when.
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;      // e.g., "Appointment Confirmed"
        public string EntityName { get; set; } = string.Empty;  // e.g., "Appointment"
        public int EntityId { get; set; }                       // The ID of the record changed
        public int PerformedByUserId { get; set; }              // Who did it
        public string Details { get; set; } = string.Empty;     // Any extra info
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
