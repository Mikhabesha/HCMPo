using System.ComponentModel.DataAnnotations;

namespace HCMPo.Models
{
    public enum TaxType
    {
        IncomeTax,
        Pension,
        Other
    }

    public class TaxSetting : IValidatableObject
    {
        public int Id { get; set; }
        public TaxType Type { get; set; }
        public decimal? MinSalary { get; set; } // Only for IncomeTax
        public decimal? MaxSalary { get; set; } // Only for IncomeTax
        [Range(0, 100)]
        public decimal Percentage { get; set; }
        public bool IsActive { get; set; }
        public string? Name { get; set; } // Only required for Other
        public decimal? Subtraction { get; set; } // Only for IncomeTax

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == TaxType.Other && string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Name is required for 'Other' tax rules.", new[] { nameof(Name) });
            }
        }
    }
} 