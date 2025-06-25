using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class PayrollStamp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; } = 0;

        [Required]
        public string UploadedBy { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool ApplyOnBankSlip { get; set; } = true;

        public bool ApplyOnReportSlip { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        public int Width { get; set; } = 150; // Default width in pixels
        public int Height { get; set; } = 75; // Default height in pixels
        public string Position { get; set; } = "bottom-right"; // Position on slip
    }
} 