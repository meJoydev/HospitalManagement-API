using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Domain.Entities
{
    // Every person in our system (admin, doctor, patient) has a User record.
    // This is the central login table.
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // We NEVER store plain text passwords. This stores the hashed version.
        // it stores values as scrambled string "$2a$11$xK9z..." that can never be reversed back to the original.
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }      // Uses enum values that Stores 1, 2, or 3 in the database
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Instead of deleting a user from the database (which causes problems if other tables reference them)
        // we just set IsActive = false, This is called a soft delete
        public bool IsActive { get; set; } = true;  

        // Navigation properties — EF Core uses these to join tables
        // They don't become columns in the Users table — instead, they tell EF Core that this User is linked to a Doctor or Patient record in another table.
        public Doctor? Doctor { get; set; }    // If role is Doctor, this links to Doctor table
        public Patient? Patient { get; set; }  // If role is Patient, this links to Patient table
    }
}