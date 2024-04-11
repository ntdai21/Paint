using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Paint.ValidationRules
{
    class NumberValidationRule : ValidationRule
    {
        public float? Min { get; set; } = null;
        public float? Max { get; set; } = null;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (string)value ?? "";
            string numberPattern = @"^[-+]?[0-9]*\.?[0-9]+$";

            if (Regex.IsMatch(input, numberPattern))
            {
                var number = double.Parse(input);

                if (Min != null && Max != null)
                {
                    if (number < Min || number > Max) return new ValidationResult(false, $"Must be in range [{Min}-{Max}].");
                }

                else if (Min != null)
                {
                    if (number < Min) return new ValidationResult(false, $"Must be equal or greater than {Min}.");
                }
                else if (Max != null)
                {
                    if (number > Max) return new ValidationResult(false, $"Must be equal or less than {Max}.");
                }

                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Number only allowed.");
        }
    }
}
