using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using DataModel.Input;

namespace Assess.Helpers.DataValidation
{
    public class CriterionNameValidationRule : ValidationRule
    {
        public CollectionViewSource CriteriaCollectionViewSource { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = (string)value;
            var trimmedName = name?.Trim(' ');
            if (string.IsNullOrEmpty(trimmedName))
                return new ValidationResult(false, "Criterion name cannot be empty!");

            if (trimmedName != name)
                return new ValidationResult(false, "Using whitespaces around criterion name is forbidden!");

            var criteriaCollection = (ObservableCollection<Criterion>)CriteriaCollectionViewSource.Source;
            if (criteriaCollection.Any(criterion => criterion.Name == name))
                return new ValidationResult(false, "Criterion already exists!");

            return ValidationResult.ValidResult;
        }
    }
}