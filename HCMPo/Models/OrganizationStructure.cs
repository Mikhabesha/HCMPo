using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    /// <summary>
    /// Enhanced organizational structure for managing directorates and hierarchical levels
    /// </summary>
    public class Directorate
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Hierarchy support
        public int Level { get; set; } = 1; // 1=Top level directorate
        public string? ParentDirectorateId { get; set; }
        public virtual Directorate? ParentDirectorate { get; set; }
        public virtual ICollection<Directorate> SubDirectorates { get; set; } = new HashSet<Directorate>();

        // Navigation properties
        // public virtual ICollection<OrganizationUnit> OrganizationUnits { get; set; } = new HashSet<OrganizationUnit>();
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }

    /// <summary>
    /// Enhanced role hierarchy system for organizational management
    /// </summary>
    public class RoleHierarchy
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Hierarchy level (1=lowest, 10=highest)
        public int HierarchyLevel { get; set; }

        // Can approve levels below this
        public int MaxApprovalLevel { get; set; }

        public bool IsActive { get; set; } = true;

        // Permissions
        public bool CanApproveLeave { get; set; }
        public bool CanManageAttendance { get; set; }
        public bool CanAccessPayroll { get; set; }
        public bool CanManageEmployees { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Employee positions within the organizational hierarchy
    /// </summary>
    public class EmployeePosition
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        public virtual Employee Employee { get; set; } = null!;

        [Required]
        public string DirectorateId { get; set; } = string.Empty;
        public virtual Directorate Directorate { get; set; } = null!;

        public string? OrganizationUnitId { get; set; }
        public virtual OrganizationUnit? OrganizationUnit { get; set; }

        [Required]
        public string RoleHierarchyId { get; set; } = string.Empty;
        public virtual RoleHierarchy RoleHierarchy { get; set; } = null!;

        // Reporting structure
        public string? SupervisorEmployeeId { get; set; }
        public virtual Employee? SupervisorEmployee { get; set; }

        // Position details
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public bool IsCurrentPosition { get; set; } = true;
        public bool IsPrimaryPosition { get; set; } = true;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Employee migration log for tracking ZKTime imports
    /// </summary>
    public class EmployeeMigrationLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Source data
        public string? SourceSystemId { get; set; } // Badge number from ZKTime
        public string? SourceSystemName { get; set; } = "ZKTime";
        public string? SourceData { get; set; } // JSON of original data

        // Migration results
        public string? EmployeeId { get; set; } // Created employee ID
        public string? ApplicationUserId { get; set; } // Created user ID
        public string? DefaultPassword { get; set; } // Generated password (hashed)

        // Status tracking
        public MigrationStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? WarningMessage { get; set; }

        // Processed data
        public string? AssignedDirectorate { get; set; }
        public string? AssignedOrganizationUnit { get; set; }
        public string? AssignedRole { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }

    public enum MigrationStatus
    {
        Pending,
        Processing,
        Success,
        PartialSuccess,
        Failed,
        Duplicate,
        RequiresReview
    }
} 