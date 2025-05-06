using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HCMPo.Models
{
    public class Employee
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Salary { get; set; }

        [Required]
        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; }

        [Required]
        [Display(Name = "Emergency Phone")]
        public string EmergencyPhone { get; set; }

        public string UserId { get; set; }
        public virtual JobTitle JobTitle { get; set; }
        public string JobTitleId { get; set; }
        public virtual Department Department { get; set; }
        public string DepartmentId { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [StringLength(100)]
        public string AmharicFirstName { get; set; }

        [StringLength(100)]
        public string AmharicLastName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string PhotoUrl { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Required]
        public DateTime EmploymentDate { get; set; }

        public string EmploymentDateEt { get; set; } // Ethiopian Calendar Date

        public string SupervisorId { get; set; }
        public virtual Employee Supervisor { get; set; }

        [Required]
        public EmploymentStatus Status { get; set; }

        public string Address { get; set; }
        public string City { get; set; }

        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }

        public string TinNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
        public string DateOfBirthEt { get; set; } // Ethiopian Calendar Date

        public string Gender { get; set; }

        public DateTime? TerminationDate { get; set; }
        public string TerminationDateEt { get; set; }

        public virtual ICollection<EmployeeLeave> Leaves { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<Payroll> Payrolls { get; set; }

        // Navigation property to Identity User
        public virtual ApplicationUser User { get; set; }
    }

    public enum EmploymentStatus
    {
        Active,
        OnLeave,
        Suspended,
        Terminated
    }
} 