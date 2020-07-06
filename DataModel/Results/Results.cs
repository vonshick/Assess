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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Results
{
    public class Results : INotifyPropertyChanged
    {
        private ObservableCollection<CriterionCoefficient> _criteriaCoefficients;
        private double? _k;

        private string _formula;
        
        private List<PartialUtility> _partialUtilityFunctions;

        public Results()
        {
            FinalRanking = new FinalRanking();
            PartialUtilityFunctions = new List<PartialUtility>();
            CriteriaCoefficients = new ObservableCollection<CriterionCoefficient>();
        }

        public FinalRanking FinalRanking { get; set; }

        public double? K
        {
            get => _k;
            set
            {
                if (Nullable.Equals(value, _k)) return;
                _k = value;
                OnPropertyChanged(nameof(K));
            }
        }

        public string Formula
        {
            get => _formula;
            set
            {
                if (Nullable.Equals(value, _formula)) return;
                _formula = value;
                OnPropertyChanged(nameof(Formula));
            }
        }

        public ObservableCollection<CriterionCoefficient> CriteriaCoefficients
        {
            get => _criteriaCoefficients;
            set
            {
                _criteriaCoefficients = value;
                OnPropertyChanged(nameof(CriteriaCoefficients));
            }
        }

        public List<PartialUtility> PartialUtilityFunctions
        {
            get => _partialUtilityFunctions;
            set
            {
                _partialUtilityFunctions = value;
                OnPropertyChanged(nameof(PartialUtilityFunctions));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void Reset()
        {
            FinalRanking.FinalRankingCollection.Clear();
            PartialUtilityFunctions.Clear();
            CriteriaCoefficients.Clear();
            K = null;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}