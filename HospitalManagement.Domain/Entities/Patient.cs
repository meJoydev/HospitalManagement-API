using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Entities
{
    // Extra patient-specific details
    public class Patient
    {
        public int Id { get; set; }

        public int UserId { get; set; }         // Foreign key
        public User User { get; set; } = null!; // Navigation Property (Usefule for retriving any data by EF core without writing any SQL joins)

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }  // ? means this column is optional (nullable)
        public string? MedicalHistory { get; set; }

        // One patient can have many appointments over time
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
