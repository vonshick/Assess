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
using CalculationsEngine.Maintenance;
using DataModel.Annotations;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class Dialog : INotifyPropertyChanged
    {
        private PartialUtilityValues _pointToAdd;
        public DisplayObject DisplayObject;
        protected double LowerUtilityBoundary;
        protected double UpperUtilityBoundary;


        public Dialog(DisplayObject displayObject)
        {
            DisplayObject = displayObject;
        }

        public Dialog(double lowerUtilityBoundary, double upperUtilityBoundary, DisplayObject displayObject)
        {
            LowerUtilityBoundary = lowerUtilityBoundary;
            UpperUtilityBoundary = upperUtilityBoundary;
            DisplayObject = displayObject;
        }

        protected Dialog()
        {
        }


        public PartialUtilityValues PointToAdd
        {
            get => _pointToAdd;
            set
            {
                if (Equals(value, _pointToAdd)) return;
                _pointToAdd = value;
                OnPropertyChanged(nameof(PointToAdd));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public virtual void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary)
        {
        }

        protected virtual void SetInitialValues()
        {
        }

        protected virtual void SetValuesIfLotteryChosen()
        {
        }

        protected virtual void SetValuesIfSureChosen()
        {
        }

        protected virtual void SetValuesIfEqualChosen()
        {
        }

        protected void UpdateLotteryComparisonPointToAdd()
        {
            PointToAdd.X = DisplayObject.ComparisonLottery.UpperUtilityValue.X;
            PointToAdd.Y = DisplayObject.EdgeValuesLottery.P / DisplayObject.ComparisonLottery.P;
        }

        protected void UpdateOtherMethodsPointToAdd()
        {
            PointToAdd.X = DisplayObject.X;
            PointToAdd.Y = DisplayObject.Lottery.NewPointUtility();
        }

        public void ProcessDialog(int choice)
        {
            if (choice == 1)
            {
                SetValuesIfSureChosen();
                SetInitialValues();
            }
            else if (choice == 2)
            {
                SetValuesIfLotteryChosen();
                SetInitialValues();
            }
            else if (choice == 3)
            {
                SetValuesIfEqualChosen();
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}