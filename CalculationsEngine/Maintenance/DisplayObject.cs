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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Models;
using DataModel.Annotations;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Maintenance
{
    public class DisplayObject : INotifyPropertyChanged
    {
        private Lottery _comparisonLottery;
        private Lottery _edgeValuesLottery;
        private Lottery _lottery;
        private double _p;
        private double _x;
        public List<PartialUtilityValues> PointsList;


        public DisplayObject()
        {
            PointsList = new List<PartialUtilityValues>();
        }

        // used in CoefficientsDialog
        public DisplayObject(IReadOnlyList<Criterion> criterionList, IReadOnlyList<double> valuesToCompare, double p)
        {
            P = p;
            CoefficientsDialogValuesList = new List<CoefficientsDialogValues>();
            for (var i = 0; i < criterionList.Count; i++)
                CoefficientsDialogValuesList.Add(new CoefficientsDialogValues(
                    valuesToCompare[i],
                    criterionList[i].MaxValue,
                    criterionList[i].MinValue,
                    criterionList[i].Name
                ));
        }


        public Lottery Lottery
        {
            get => _lottery;
            set
            {
                if (Equals(value, _lottery)) return;
                _lottery = value;
                OnPropertyChanged(nameof(Lottery));
            }
        }

        public double X
        {
            get => _x;
            set
            {
                if (value.Equals(_x)) return;
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public double P //lotteries comparison dialog, constant probability dialog, coefficients dialog (starts with 0.5 by default)
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

        public Lottery ComparisonLottery //lotteries comparison
        {
            get => _comparisonLottery;
            set
            {
                _comparisonLottery = value;
                OnPropertyChanged(nameof(ComparisonLottery));
            }
        }

        public Lottery EdgeValuesLottery //lotteries comparison
        {
            get => _edgeValuesLottery;
            set
            {
                _edgeValuesLottery = value;
                OnPropertyChanged(nameof(EdgeValuesLottery));
            }
        }

        public List<CoefficientsDialogValues> CoefficientsDialogValuesList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}