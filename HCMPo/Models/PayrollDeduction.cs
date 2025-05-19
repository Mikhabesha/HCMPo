using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public class PayrollDeduction
    {
        public int Id { get; set; }
        public int PayrollId { get; set; }
        public int DeductionTypeId { get; set; }
        public virtual DeductionType DeductionType { get; set; }
        public decimal Amount { get; set; }
        public virtual Payroll Payroll { get; set; }
    }
} 