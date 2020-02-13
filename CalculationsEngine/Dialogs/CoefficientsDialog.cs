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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Maintenance;
using DataModel.Annotations;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class CoefficientsDialog : INotifyPropertyChanged
    {
        private readonly List<double> _bestValues;
        private readonly List<Criterion> _criterionList;
        private readonly List<double> _worstValues;
        private string _currentCriterionName;
        private DisplayObject _displayObject;
        private double _lowerProbabilityBoundary;
        private double _upperProbabilityBoundary;
        public List<CriterionCoefficient> CriteriaCoefficientsList;

        public CoefficientsDialog(List<Criterion> criterionList)
        {
            _criterionList = criterionList;
            _bestValues = new List<double>();
            _worstValues = new List<double>();
            CriteriaCoefficientsList = new List<CriterionCoefficient>();

            for (var i = 0; i < _criterionList.Count; i++)
                if (_criterionList[i].CriterionDirection == "Cost")
                {
                    _bestValues.Add(_criterionList[i].MinValue);
                    _worstValues.Add(_criterionList[i].MaxValue);
                }
                else
                {
                    _bestValues.Add(_criterionList[i].MaxValue);
                    _worstValues.Add(_criterionList[i].MinValue);
                }
        }


        public DisplayObject DisplayObject
        {
            get => _displayObject;
            set
            {
                _displayObject = value;
                OnPropertyChanged(nameof(DisplayObject));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetInitialValues(Criterion criterion)
        {
            var index = _criterionList.IndexOf(criterion);
            var valuesToCompare = _worstValues.ToArray();
            valuesToCompare[index] = _bestValues[index];

            DisplayObject = new DisplayObject(_criterionList, valuesToCompare, 0.5f);

            _currentCriterionName = _criterionList[index].Name;
            _lowerProbabilityBoundary = 0;
            _upperProbabilityBoundary = 1;
        }

        public void ProcessDialog(int choice)
        {
            if (choice == 1)
            {
                _lowerProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
            }
            else if (choice == 2)
            {
                _upperProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
            }
            else if (choice == 3)
            {
                Console.WriteLine("\nDodano nowy wspolczynnik: " + _currentCriterionName + " : " + DisplayObject.P);
                CriteriaCoefficientsList.Add(new CriterionCoefficient(_currentCriterionName, DisplayObject.P));
            }
            else
            {
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}