using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using DataModel.Input;

namespace UTA.Models.DataValidation
{
    public class AlternativeNameValidationRule : ValidationRule
    {
        public CollectionViewSource AlternativesColl { get; set; }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            var bindingExpression = (BindingExpression) value;
            var alternative = (Alternative) bindingExpression.ResolvedSource;
            if (alternative.Name.Replace(" ", string.Empty) == "")
            {
                Console.WriteLine("validation FAIL - EMPTY of " + alternative.Name);
                return new ValidationResult(false,
                    "Alternative name cannot be empty!");
            }

            var alternativesCollection = (ObservableCollection<Alternative>) AlternativesColl.Source;
            foreach (var alternative1 in alternativesCollection)
                if (alternative1 != alternative && alternative1.Name == alternative.Name)
                {
                    Console.WriteLine("validation FAIL - ALREADY EXISTS of " + alternative.Name);
                    return new ValidationResult(false,
                        "Alternative exists!");
                }

            //todo not working?
            foreach (var bE in bindingExpression.BindingGroup.BindingExpressions) Validation.ClearInvalid(bE);

            Console.WriteLine("validation OK of " + alternative.Name);
            return ValidationResult.ValidResult;
        }
    }
}