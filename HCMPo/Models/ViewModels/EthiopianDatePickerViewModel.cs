namespace HCMPo.Models.ViewModels
{
    public class EthiopianDatePickerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool ShowGregorian { get; set; } = true;
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
        public int? SelectedDay { get; set; }
        public string Placeholder { get; set; } = "Select a date";
    }
} 