using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Application.DTOs.Doctor
{
    public class DoctorScheduleDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public DayOfWeek DayOfWeek { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public TimeSpan StartTime { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public TimeSpan EndTime { get; set; }
    }
}