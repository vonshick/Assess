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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Results;

namespace CalculationsEngine.Models
{
    public class Lottery : INotifyPropertyChanged
    {
        private double _p;


        public Lottery(PartialUtilityValues lowerUtilityValue, PartialUtilityValues upperUtilityValue)
        {
            LowerUtilityValue = lowerUtilityValue;
            UpperUtilityValue = upperUtilityValue;
        }


        public double P
        {
            get => _p;
            set
            {
                if (value.Equals(_p)) return;
                _p = value;
                OnPropertyChanged(nameof(P));
                OnPropertyChanged(nameof(ComplementaryP));
            }
        }

        public double ComplementaryP => 1 - P;
        public PartialUtilityValues LowerUtilityValue { get; set; }
        public PartialUtilityValues UpperUtilityValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public void SetProbability(double p)
        {
            P = p;
        }

        public double NewPointUtility()
        {
            return P * UpperUtilityValue.Y + (1 - P) * LowerUtilityValue.Y;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}