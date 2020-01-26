using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using DataModel.Input;

namespace UTA.Helpers.DataValidation
{
    public class AlternativeNameValidationRule : ValidationRule
    {
        public CollectionViewSource AlternativesCollectionViewSource { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = (string)value;
            var trimmedName = name?.Trim(' ');
            if (string.IsNullOrEmpty(trimmedName))
                return new ValidationResult(false, "Alternative name cannot be empty!");

            if (trimmedName != name)
                return new ValidationResult(false, "Using whitespaces around alternative name is forbidden!");

            var criteriaCollection = (ObservableCollection<Alternative>)AlternativesCollectionViewSource.Source;
            if (criteriaCollection.Any(criterion => criterion.Name == name))
                return new ValidationResult(false, "Alternative already exists!");

            return ValidationResult.ValidResult;
        }
    }
}