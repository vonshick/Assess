using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using DataModel.Input;

namespace UTA.Models.DataValidation
{
    internal class CriterionNameValidationRule : ValidationRule
    {
        public CollectionViewSource CriteriaColl { get; set; }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            var bindingExpression = (BindingExpression) value;
            var criterion = (Criterion) bindingExpression.ResolvedSource;
            if (criterion.Name.Replace(" ", string.Empty) == "")
            {
                Console.WriteLine("validation FAIL - EMPTY of " + criterion.Name);
                return new ValidationResult(false,
                    "Criterion name cannot be empty!");
            }

            var criteriaCollection = (ObservableCollection<Criterion>) CriteriaColl.Source;
            foreach (var criterion1 in criteriaCollection)
                if (criterion1 != criterion && criterion1.Name == criterion.Name)
                {
                    Console.WriteLine("validation FAIL - ALREADY EXISTS of " + criterion.Name);
                    return new ValidationResult(false,
                        "Criterion exists!");
                }

            //todo not working?
            foreach (var bE in bindingExpression.BindingGroup.BindingExpressions) Validation.ClearInvalid(bE);

            Console.WriteLine("validation OK of " + criterion.Name);
            return ValidationResult.ValidResult;
        }
    }
}