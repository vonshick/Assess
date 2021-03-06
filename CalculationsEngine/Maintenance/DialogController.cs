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
using CalculationsEngine.Dialogs;
using CalculationsEngine.Models;
using DataModel.Annotations;
using DataModel.Results;

namespace CalculationsEngine.Maintenance
{
    public class DialogController : INotifyPropertyChanged
    {
        private readonly int _methodId;
        private readonly PartialUtilityValues _oneUtilityPoint;
        private readonly PartialUtilityValues _zeroUtilityPoint;
        private Dialog _dialog;
        private DisplayObject _displayObject;
        public List<PartialUtilityValues> PointsList;


        // methodId - integer from 1 - 4
        // 1 - constant probability 
        // 2 - variable probability
        // 3 - lotteries comparison
        // 4 - probability comparison
        public DialogController(PartialUtility partialUtility, int methodId, double p = 0.5)
        {
            _methodId = methodId;
            DisplayObject = new DisplayObject();
            PointsList = partialUtility.PointsValues;
            DisplayObject.PointsList = PointsList;
            DisplayObject.P = p;
            _zeroUtilityPoint = partialUtility.PointsValues.Find(o => o.Y == 0);
            _oneUtilityPoint = partialUtility.PointsValues.Find(o => o.Y == 1);

            if (_methodId == 1) createConstantProbabilityObject();
            else if (_methodId == 2) createVariableProbabilityObject();
            else if (_methodId == 3) createLotteriesComparisonObject();
            else if (_methodId == 4) createProbabilityComparisonObject();
        }


        public Dialog Dialog
        {
            get => _dialog;
            set
            {
                if (Equals(value, _dialog)) return;
                _dialog = value;
                OnPropertyChanged(nameof(Dialog));
            }
        }

        public DisplayObject DisplayObject
        {
            get => _displayObject;
            set
            {
                if (Equals(value, _displayObject)) return;
                _displayObject = value;
                OnPropertyChanged(nameof(DisplayObject));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        // firstPoint and secondPoint are edges of chosen utility function segment
        public Dialog TriggerDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            if (_methodId == 1) return triggerConstantProbabilityDialog(firstPoint, secondPoint);
            if (_methodId == 2) return triggerVariableProbabilityDialog(firstPoint, secondPoint);
            if (_methodId == 3) return triggerLotteriesComparisonDialog(firstPoint, secondPoint);
            if (_methodId == 4) return triggerProbabilityComparisonDialog(firstPoint, secondPoint);
            return null;
        }


        private void setLotteriesComparisonInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            var edgeValuesLottery = new Lottery(_zeroUtilityPoint, _oneUtilityPoint);
            edgeValuesLottery.SetProbability((upperUtilityPoint.Y + lowerUtilityPoint.Y) / 2 * DisplayObject.P);

            var mediumUtilityPoint = new PartialUtilityValues((upperUtilityPoint.X + lowerUtilityPoint.X) / 2, -1);
            var comparisonLottery = new Lottery(_zeroUtilityPoint, mediumUtilityPoint);
            comparisonLottery.SetProbability(DisplayObject.P);

            DisplayObject.EdgeValuesLottery = edgeValuesLottery;
            DisplayObject.ComparisonLottery = comparisonLottery;

            Dialog.SetInitialDialogValues(
                lowerUtilityPoint.Y * DisplayObject.P,
                upperUtilityPoint.Y * DisplayObject.P,
                lowerUtilityPoint.Y,
                upperUtilityPoint.Y
            );
        }

        private void createLotteriesComparisonObject()
        {
            Dialog = new LotteriesComparisonDialog(0, DisplayObject.P, DisplayObject);
            setLotteriesComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
        }

        public Dialog triggerLotteriesComparisonDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setLotteriesComparisonInput(firstPoint, secondPoint);
            return Dialog;
        }


        private void setProbabilityComparisonInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(_zeroUtilityPoint, _oneUtilityPoint);
            DisplayObject.X = (firstPoint.X + secondPoint.X) / 2;
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.Y + upperUtilityPoint.Y) / 2);

            Dialog.SetInitialDialogValues(lowerUtilityPoint.Y, upperUtilityPoint.Y, lowerUtilityPoint.Y, upperUtilityPoint.Y);
        }

        private void createProbabilityComparisonObject()
        {
            Dialog = new ProbabilityComparisonDialog(_zeroUtilityPoint.Y, _oneUtilityPoint.Y, DisplayObject);
            setProbabilityComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
        }

        public Dialog triggerProbabilityComparisonDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setProbabilityComparisonInput(firstPoint, secondPoint);
            return Dialog;
        }


        private void setConstantProbabilityInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            double min, max;
            if (lowerUtilityPoint.X < upperUtilityPoint.X)
            {
                min = lowerUtilityPoint.X;
                max = upperUtilityPoint.X;
            }
            else
            {
                min = upperUtilityPoint.X;
                max = lowerUtilityPoint.X;
            }

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability(DisplayObject.P);

            Dialog.SetInitialDialogValues(lowerUtilityPoint.X, upperUtilityPoint.X, min, max);
        }

        private void createConstantProbabilityObject()
        {
            Dialog = new ConstantProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
            setConstantProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
        }

        public Dialog triggerConstantProbabilityDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setConstantProbabilityInput(firstPoint, secondPoint);
            return Dialog;
        }


        private void setVariableProbabilityInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            double min, max;
            if (lowerUtilityPoint.X < upperUtilityPoint.X)
            {
                min = lowerUtilityPoint.X;
                max = upperUtilityPoint.X;
            }
            else
            {
                min = upperUtilityPoint.X;
                max = lowerUtilityPoint.X;
            }

            DisplayObject.Lottery = new Lottery(_zeroUtilityPoint, _oneUtilityPoint);
            DisplayObject.X = (firstPoint.X + secondPoint.X) / 2;
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.Y + upperUtilityPoint.Y) / 2);

            Dialog.SetInitialDialogValues(lowerUtilityPoint.X, upperUtilityPoint.X, min, max);
        }

        private void createVariableProbabilityObject()
        {
            Dialog = new VariableProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
            setVariableProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
        }

        public Dialog triggerVariableProbabilityDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setVariableProbabilityInput(firstPoint, secondPoint);
            return Dialog;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}