using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Enums
{
    // This is the "state machine" for appointments.
    // An appointment can only move through these states in a specific order.
    // Requested → Confirmed → Completed
    // Requested → Cancelled
    // Confirmed → Cancelled

    // By using an enum, we make it impossible to set a status like "Booked" or "Done" or anything outside these 4 values.
    public enum AppointmentStatus
    {
        Requested = 1,    // Patient just booked it
        Confirmed = 2,    // Doctor confirmed it
        Completed = 3,    // Appointment happened
        Cancelled = 4     // Either party cancelled
    }
}
