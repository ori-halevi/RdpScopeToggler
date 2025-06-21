using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RdpScopeToggler.Validators
{
    public class RangeValidationRule : ValidationRule
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult(false, "Value is required.");

            if (int.TryParse(value.ToString(), out int number))
            {
                if (number < Minimum || number > Maximum)
                    return new ValidationResult(false, $"Value must be between {Minimum} and {Maximum}.");
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Invalid number.");
        }
    }
}
