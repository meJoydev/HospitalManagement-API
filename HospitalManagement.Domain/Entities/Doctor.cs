using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Entities
{
    // Extra doctor-specific details beyond the basic User record
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }         // Foreign key — links back to the Users table
        public User User { get; set; } = null!; // Navigation Property (Usefule for retriving any data by EF core without writing any SQL joins)

        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int MaxDailyAppointments { get; set; } = 10;

        // Navigation: one doctor has many appointments and many schedules (One to Many Relationships)
        // ICollection means "a group of multiple things"
        // new List<Appointment>() initializes it as an empty list by default, prevents null reference errors if you access it before EF Core loads the data.
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    }
}
