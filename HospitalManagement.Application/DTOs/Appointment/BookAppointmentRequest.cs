using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Application.DTOs.Appointment
{
    // Patient sends this when booking an appointment
    // Notice we don't ask for PatientId, we get that from the JWT token automatically in the controller. The patient can't fake someone else's ID this way.
    public class BookAppointmentRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int DoctorId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime AppointmentDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
