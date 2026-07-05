using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        // LogAsync Takes 5 pieces of information and writes one row to the AuditLogs table. Returns nothing (Task with no type inside)
        Task LogAsync(string action, string entityName, int entityId, int userId, string details);
    }
}
