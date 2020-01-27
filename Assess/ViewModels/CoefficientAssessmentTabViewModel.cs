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
using System.Windows;
using Assess.Models.Tab;
using Assess.Properties;
using CalculationsEngine.Dialogs;
using DataModel.Input;
using DataModel.Results;

namespace Assess.ViewModels
{
    public class CoefficientAssessmentTabViewModel : Tab, INotifyPropertyChanged
    {
        private readonly List<Criterion> _criteriaCollection;
        private readonly Action _dialogEndAction;
        private int _currentCriterionIndex;
        private readonly Results _results;


        public CoefficientAssessmentTabViewModel(List<Criterion> criteriaCollection,
            Results results, Action dialogEndAction)
        {
            Name = "Scaling Coefficient - Dialogue";
            _criteriaCollection = criteriaCollection;
            _results = results;
            _dialogEndAction = dialogEndAction;
            Dialog = new CoefficientsDialog(criteriaCollection);
            Dialog.SetInitialValues(_criteriaCollection[_currentCriterionIndex = 0]);
        }


        public CoefficientsDialog Dialog { get; set; }
        public Criterion CurrentCriterion => _criteriaCollection[_currentCriterionIndex];

        public event PropertyChangedEventHandler PropertyChanged;


        [UsedImplicitly]
        public void CertaintyOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(1);
        }

        [UsedImplicitly]
        public void LotteryOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(2);
        }

        [UsedImplicitly]
        public void IndifferentOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(3);
            if (_currentCriterionIndex < _criteriaCollection.Count - 1)
            {
                Dialog.SetInitialValues(_criteriaCollection[++_currentCriterionIndex]);
                OnPropertyChanged(nameof(CurrentCriterion));
            }
            else
            {
                // update Results.CriteriaCoefficients with new coefficients
                _results.CriteriaCoefficients = new ObservableCollection<CriterionCoefficient>(Dialog.CriteriaCoefficientsList);
                _dialogEndAction();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}