// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

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