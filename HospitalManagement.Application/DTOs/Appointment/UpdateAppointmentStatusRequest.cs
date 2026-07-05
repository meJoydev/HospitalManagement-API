using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.DTOs.Appointment
{
    // Doctor or Admin sends this to confirm/cancel/complete an appointment
    public class UpdateAppointmentStatusRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public AppointmentStatus NewStatus { get; set; }

        public string? Notes { get; set; }
    }
}
