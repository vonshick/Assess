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
            var validatedAlternative = (Alternative) bindingExpression.ResolvedSource;
            if (validatedAlternative.Name.Replace(" ", string.Empty) == "")
            {
                Console.WriteLine("validation FAILED - EMPTY name in " + validatedAlternative.Name);
                return new ValidationResult(false,
                    "Alternative name cannot be empty!");
            }

            var alternativesCollection = (ObservableCollection<Alternative>) AlternativesColl.Source;
            foreach (var alternativeFromCollection in alternativesCollection)
                if (alternativeFromCollection != validatedAlternative && alternativeFromCollection.Name == validatedAlternative.Name)
                {
                    Console.WriteLine("validation FAILED: " + validatedAlternative.Name + " ALREADY EXISTS");
                    return new ValidationResult(false,
                        "Alternative already exists!");
                }

            Console.WriteLine("validation OK of " + validatedAlternative.Name);
            return ValidationResult.ValidResult;
        }
    }
}