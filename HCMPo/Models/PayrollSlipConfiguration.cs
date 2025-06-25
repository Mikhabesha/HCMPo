using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class PayrollSlipConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; }

        [StringLength(500)]
        public string CompanyAddress { get; set; }

        [StringLength(100)]
        public string CompanyPhone { get; set; }

        [StringLength(100)]
        public string CompanyEmail { get; set; }

        public string CompanyLogo { get; set; } // File path to company logo

        [StringLength(255)]
        public string BankSlipTitle { get; set; } = "Bank Payment Slip";

        [StringLength(255)]
        public string ReportSlipTitle { get; set; } = "Payroll Report Slip";

        public bool ShowEmployeePhoto { get; set; } = true;

        public bool ShowQRCode { get; set; } = false;

        public bool ShowBarcode { get; set; } = false;

        [StringLength(1000)]
        public string FooterText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
} 