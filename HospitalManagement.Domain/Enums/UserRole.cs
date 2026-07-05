using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Enums
{
    // This enum defines what type of user someone is.
    // Enums are better than storing strings like "Admin" in DB — safer and faster.
    // what if someone types "admin" or "ADMIN"? that's why we store a number
    public enum UserRole
    {
        Admin = 1,
        Doctor = 2,
        Patient = 3
    }
}
