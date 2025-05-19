namespace HCMPo.ViewModels
{
    public class PayrollPeriodGroupViewModel
    {
        public string PayPeriodStartEt { get; set; }
        public string PayPeriodEndEt { get; set; }
        public string ApprovedBy { get; set; }
        public int PayrollCount { get; set; }
        public string PeriodId { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsOpened { get; set; }
    }
} 