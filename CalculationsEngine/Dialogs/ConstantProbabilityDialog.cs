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

using CalculationsEngine.Maintenance;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class ConstantProbabilityDialog : Dialog
    {
        public ConstantProbabilityDialog(double lowerUtilityBoundary, double upperUtilityBoundary, DisplayObject displayObject)
            : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
        {
        }


        public override void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary, double min, double max)
        {
            LowerUtilityBoundary = lowerUtilityBoundary;
            UpperUtilityBoundary = upperUtilityBoundary;
            UpdateSureValue();
            PointToAdd = new PointToAdd(DisplayObject.X, DisplayObject.Lottery.NewPointUtility(), min, max, true);
        }

        private void UpdateSureValue()
        {
            DisplayObject.X = LowerUtilityBoundary + (UpperUtilityBoundary - LowerUtilityBoundary) * DisplayObject.Lottery.P;
        }

        protected override void SetValuesIfLotteryChosen()
        {
            LowerUtilityBoundary = DisplayObject.X;
            UpdateSureValue();
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfSureChosen()
        {
            UpperUtilityBoundary = DisplayObject.X;
            UpdateSureValue();
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(PointToAdd);
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}