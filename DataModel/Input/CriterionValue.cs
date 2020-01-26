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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.PropertyChangedExtended;

namespace DataModel.Input
{
    public class CriterionValue : INotifyPropertyChanged, INotifyPropertyChangedExtended<double?>
    {
        private string _name;
        private double? _value;


        public CriterionValue(string name, double? value)
        {
            Name = name;
            Value = value;
        }


        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        // null is acceptable only during initialization
        public double? Value
        {
            get => _value;
            set
            {
                if (Nullable.Equals(value, _value)) return;
                if (value == null) throw new ArgumentException("Value is required!");
                var oldValue = _value;
                _value = Math.Round((double) value, 14);
                OnPropertyChangedExtended(nameof(Value), oldValue, _value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChangedExtended(string propertyName, double? oldValue, double? newValue)
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<double?>(propertyName, oldValue, newValue));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}