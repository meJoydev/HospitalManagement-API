using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Entities
{
    // Defines when a doctor is available each week.
    // Example: Dr. Sharma works Monday and Wednesday, 9am–5pm
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!; // Navigation Property (Usefule for retriving any data by EF core without writing any SQL joins)

        public DayOfWeek DayOfWeek { get; set; }    // built-in C# enum i.e, Monday, Tuesday, etc.
        public TimeSpan StartTime { get; set; }     // built-in C# enum e.g., 09:00:00
        public TimeSpan EndTime { get; set; }       // e.g., 17:00:00
        public bool IsAvailable { get; set; } = true;
    }
}