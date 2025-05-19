using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public enum PayrollDeclarationStatus
    {
        PendingAccountantReview,
        Approved,
        Rejected
    }

    public class PayrollDeclaration
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        // Store employee IDs as a comma-separated string for simplicity
        [Required]
        public string EmployeeIds { get; set; } = string.Empty;

        [Required]
        public PayrollDeclarationStatus Status { get; set; } = PayrollDeclarationStatus.PendingAccountantReview;

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Comments { get; set; }
    }
} 