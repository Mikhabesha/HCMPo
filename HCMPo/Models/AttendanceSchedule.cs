using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPo.Models
{
    public class AttendanceSchedule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public bool IsFixedSchedule { get; set; }
        
        public string? MondayScheduleId { get; set; }
        public string? TuesdayScheduleId { get; set; }
        public string? WednesdayScheduleId { get; set; }
        public string? ThursdayScheduleId { get; set; }
        public string? FridayScheduleId { get; set; }
        public string? SaturdayScheduleId { get; set; }
        public string? SundayScheduleId { get; set; }
        
        public virtual ICollection<Employee> Employees { get; set; }
        
        public AttendanceSchedule()
        {
            Employees = new HashSet<Employee>();
        }
    }
} 