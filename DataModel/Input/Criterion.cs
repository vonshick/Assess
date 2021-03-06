﻿// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.PropertyChangedExtended;

namespace DataModel.Input
{
    public class Criterion : INotifyPropertyChanged, INotifyPropertyChangedExtended<string>
    {
        private string _criterionDirection;
        private string _description;
        private double _minValue = double.MaxValue;
        private double _maxValue = double.MinValue;
        private string _method = "Set during calculations";
        private string _name;
        private double? _p = 0.5;
        private bool _disabled;


        public Criterion()
        {
        }

        public Criterion(string name, string criterionDirection)
        {
            Name = name;
            CriterionDirection = criterionDirection;
        }

        public Criterion(string name, string description, string criterionDirection, double? probability = 0.5,
            string method = "Set during calculations", bool disabled = false)
        {
            Name = name;
            Description = description;
            CriterionDirection = criterionDirection;
            Probability = probability;
            Method = method;
            Disabled = disabled;
        }


        public static double MinimumProbability { get; } = 1E-15;
        public static double MaximumProbability { get; } = 1 - 1E-15;

        [UsedImplicitly] public static string[] CriterionDirectionTypesList { get; } = {"Gain", "Cost"};

        [UsedImplicitly] // changing order of options or renaming them will break application
        public static List<string> MethodOptionsList { get; } = new List<string>
        {
            "Set during calculations",
            "Certainty equivalent with constant probability",
            "Certainty equivalent with variable probability",
            "Lottery comparison",
            "Probability comparison"
        };

        public string ID { get; set; }

        public double MinValue
        {
            get => _minValue;
            set => _minValue = Math.Round(value, 14);
        }

        public double MaxValue
        {
            get => _maxValue;
            set => _maxValue = Math.Round(value, 14);
        }

        public bool IsEnum { get; set; } = false;

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                var oldValue = _name;
                _name = value;
                OnPropertyChangedExtended(nameof(Name), oldValue, _name);
            }
        }

        public string CriterionDirection
        {
            get => _criterionDirection;
            set
            {
                if (value == _criterionDirection) return;
                if (value != "Cost" && value != "Gain")
                    throw new ArgumentException("Value must be \"Cost\" or \"Gain\".");
                _criterionDirection = value;
                OnPropertyChanged(nameof(CriterionDirection));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public double? Probability // used in lotteries comparison dialog, constant probability dialog
        {
            get => IsProbabilityIncluded ? _p : null;
            set
            {
                if (value.Equals(_p) || value == null) return;
                if (IsProbabilityIncluded && (value == null || value <= 0 || value >= 1))
                    throw new ArgumentException("Value must be between 0 and 1 exclusive.");
                _p = value;
                OnPropertyChanged(nameof(Probability));
            }
        }

        public bool IsProbabilityIncluded => Method == MethodOptionsList[1] || Method == MethodOptionsList[3];

        public string Method
        {
            get => _method;
            set
            {
                if (value == _method) return;
                if (!MethodOptionsList.Contains(value))
                    throw new ArgumentException(MethodOptionsList.Aggregate("Value must be one of the following:",
                        (current, option) => current + $"\n{option}"));
                _method = value;
                OnPropertyChanged(nameof(Method));
                OnPropertyChanged(nameof(IsProbabilityIncluded));
                OnPropertyChanged(nameof(Probability));
            }
        }

        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (value == _disabled) return;
                _disabled = value;
                OnPropertyChanged(nameof(Disabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChangedExtended(string propertyName, string oldValue, string newValue)
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<string>(propertyName, oldValue, newValue));
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