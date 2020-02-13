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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Assess.Helpers;
using Assess.Models.Tab;
using Assess.Properties;
using CalculationsEngine.Maintenance;
using DataModel.Input;
using DataModel.Results;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Assess.ViewModels
{
    public class PartialUtilityTabViewModel : Tab, INotifyPropertyChanged
    {
        private readonly Action _calculateUtilities;
        private readonly OxyColor _colorPrimarySelected = OxyColor.FromArgb(80, 51, 115, 242);
        private readonly OxyColor _colorPrimaryUnselected = OxyColor.FromArgb(30, 51, 115, 242);
        private readonly OxyColor _gridColor = OxyColor.FromRgb(240, 240, 240); // ColorInterface5
        private readonly List<PartialUtilityValues> _initialPointsValues;
        private readonly LineSeries _line;
        private readonly OxyColor _lineColor = OxyColor.FromRgb(110, 110, 110); // ColorSecondary
        private readonly PartialUtility _partialUtility;
        private readonly LineSeries _placeholderLine;
        private readonly OxyColor _placeholderLineColor = OxyColor.FromArgb(70, 110, 110, 110); // ColorSecondary
        private readonly OxyColor _placeholderMarkerColor = OxyColor.FromRgb(51, 115, 242); // ColorPrimary
        private readonly Action _restartCoefficientsAssessment;
        private DialogController _dialogController;
        private bool _isMethodSet;
        private bool _isSettingExactValue;
        private RectangleAnnotation _selectedRectangle;


        public PartialUtilityTabViewModel(PartialUtility partialUtility, Action calculateUtilities, Action restartCoefficientsAssessment)
        {
            _partialUtility = partialUtility;
            _calculateUtilities = calculateUtilities;
            _restartCoefficientsAssessment = restartCoefficientsAssessment;

            _initialPointsValues = new List<PartialUtilityValues>
            {
                new PartialUtilityValues(PointsValues[0].X, PointsValues[0].Y),
                new PartialUtilityValues(PointsValues.Last().X, PointsValues.Last().Y)
            };

            Name = $"{Criterion.Name} - Utility";
            Title = $"{Criterion.Name} - Partial Utility Function";

            IsMethodSet = Criterion.Method != Criterion.MethodOptionsList[0];
            if (IsMethodSet)
                DialogController = new DialogController(_partialUtility,
                    Criterion.MethodOptionsList.IndexOf(Criterion.Method), Criterion.Probability ?? 0);
            else
                // choose first method as default, to prevent from not selecting any method at all in radio buttons
                Criterion.Method = Criterion.MethodOptionsList[1];

            const double verticalAxisExtraSpace = 0.0001; // TODO: consider change
            var horizontalAxisExtraSpace = (Criterion.MaxValue - Criterion.MinValue) * 0.0001;
            // plot initializer
            _line = new LineSeries
            {
                Color = _lineColor,
                StrokeThickness = 3,
                MarkerFill = _lineColor,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6
            };

            _placeholderLine = new LineSeries
            {
                Color = _placeholderLineColor,
                StrokeThickness = 3,
                MarkerStroke = _placeholderMarkerColor,
                MarkerStrokeThickness = 2,
                MarkerFill = OxyColors.Transparent,
                MarkerType = MarkerType.Circle,
                MarkerSize = 9
            };

            PlotModel = new ViewResolvingPlotModel
            {
                Series = {_line, _placeholderLine},
                DefaultFont = "Segoe UI",
                DefaultFontSize = 14,
                Padding = new OxyThickness(0, 0, 0, 0),
                PlotAreaBackground = OxyColors.White,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Partial Utility",
                        FontSize = 16,
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = _gridColor,
                        AbsoluteMinimum = 0 - verticalAxisExtraSpace,
                        AbsoluteMaximum = 1 + verticalAxisExtraSpace,
                        Minimum = 0 - verticalAxisExtraSpace,
                        Maximum = 1 + verticalAxisExtraSpace,
                        MajorTickSize = 8,
                        IntervalLength = 30,
                        AxisTitleDistance = 12
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        Title = "Criterion Value",
                        FontSize = 16,
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = _gridColor,
                        AbsoluteMinimum = Criterion.MinValue - horizontalAxisExtraSpace,
                        AbsoluteMaximum = Criterion.MaxValue + horizontalAxisExtraSpace,
                        Minimum = Criterion.MinValue - horizontalAxisExtraSpace,
                        Maximum = Criterion.MaxValue + horizontalAxisExtraSpace,
                        MajorTickSize = 8,
                        AxisTitleDistance = 4
                    }
                }
            };
            PlotModel.MouseDown += (sender, args) =>
            {
                if (args.ChangedButton != OxyMouseButton.Left) return;
                if (SelectedRectangle != null) SelectedRectangle.Fill = _colorPrimaryUnselected;
                SelectedRectangle = null;
                _placeholderLine.Points.Clear();
                PlotModel.InvalidatePlot(false);
            };
            InitializePlotIfMethodIsSet();
        }


        // switches ContentControl content for dialogue.
        // nullable because it only switches ContentControl when DisplayObject is initialized and data is bound
        public bool? IsLotteryComparison => DialogController == null ? (bool?) null : Criterion.Method == Criterion.MethodOptionsList[3];

        public bool? IsNewPointProcessedVertically => DialogController == null
            ? (bool?) null
            : Criterion.Method == Criterion.MethodOptionsList[3] || Criterion.Method == Criterion.MethodOptionsList[4];

        public IEnumerable<string> Methods { get; } = Criterion.MethodOptionsList.Skip(1);
        public string Title { get; }
        public ViewResolvingPlotModel PlotModel { get; }
        public Criterion Criterion => _partialUtility.Criterion;
        private List<PartialUtilityValues> PointsValues => _partialUtility.PointsValues;

        public DialogController DialogController
        {
            get => _dialogController;
            set
            {
                _dialogController = value;
                OnPropertyChanged(nameof(DialogController));
                OnPropertyChanged(nameof(IsLotteryComparison));
                OnPropertyChanged(nameof(IsNewPointProcessedVertically));
            }
        }

        public RectangleAnnotation SelectedRectangle
        {
            get => _selectedRectangle;
            set
            {
                _selectedRectangle = value;
                OnPropertyChanged(nameof(SelectedRectangle));
            }
        }

        public bool IsMethodSet
        {
            get => _isMethodSet;
            set
            {
                if (value == _isMethodSet) return;
                _isMethodSet = value;
                OnPropertyChanged(nameof(IsMethodSet));
            }
        }

        public bool IsSettingExactValue
        {
            get => _isSettingExactValue;
            set
            {
                if (value == _isSettingExactValue) return;
                _isSettingExactValue = value;
                OnPropertyChanged(nameof(IsSettingExactValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitializePlotIfMethodIsSet()
        {
            if (!IsMethodSet) return;
            GeneratePlotData();
            if (PlotModel.Annotations.Count == 1) SelectRectangle((RectangleAnnotation) PlotModel.Annotations[0], 0);
            PlotModel.InvalidatePlot(false);
        }

        public void GeneratePlotData()
        {
            _line.Points.Clear();
            _placeholderLine.Points.Clear();
            PlotModel.Annotations.Clear();
            SelectedRectangle = null;

            for (var i = 0; i < PointsValues.Count; i++)
            {
                if (i < PointsValues.Count - 1)
                {
                    var rectangle = new RectangleAnnotation
                    {
                        MinimumX = PointsValues[i].X,
                        MaximumX = PointsValues[i + 1].X,
                        MinimumY = PointsValues[i].Y,
                        MaximumY = PointsValues[i + 1].Y,
                        Fill = _colorPrimaryUnselected,
                        Layer = AnnotationLayer.BelowSeries
                    };
                    rectangle.MouseDown += RectangleOnMouseDown;
                    PlotModel.Annotations.Add(rectangle);
                }

                _line.Points.Add(new DataPoint(PointsValues[i].X, PointsValues[i].Y));
            }

            PlotModel.InvalidatePlot(false);
        }

        private void RectangleOnMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left) return;
            var rectangle = (RectangleAnnotation) sender;
            if (SelectedRectangle == rectangle)
            {
                PlotEventHandler(e);
                return;
            }

            var firstPointIndex = PointsValues.FindIndex(partialUtilityValue => partialUtilityValue.X == rectangle.MinimumX);
            DialogController.TriggerDialog(PointsValues[firstPointIndex], PointsValues[firstPointIndex + 1]);
            SelectRectangle(rectangle, firstPointIndex);
            PlotEventHandler(e);
        }

        private void SelectRectangle(RectangleAnnotation rectangle, int firstPointIndex)
        {
            if (SelectedRectangle != null) SelectedRectangle.Fill = _colorPrimaryUnselected;
            SelectedRectangle = rectangle;
            SelectedRectangle.Fill = _colorPrimarySelected;

            _placeholderLine.Points.Clear();
            _placeholderLine.Points.Add(new DataPoint(PointsValues[firstPointIndex].X, PointsValues[firstPointIndex].Y));
            _placeholderLine.Points.Add(new DataPoint(DialogController.Dialog.PointToAdd.X, DialogController.Dialog.PointToAdd.Y));
            _placeholderLine.Points.Add(new DataPoint(PointsValues[firstPointIndex + 1].X, PointsValues[firstPointIndex + 1].Y));

            DialogController.Dialog.PointToAdd.PropertyChanged += (o, args) =>
            {
                _placeholderLine.Points[1] = new DataPoint(DialogController.Dialog.PointToAdd.X, DialogController.Dialog.PointToAdd.Y);
                PlotModel.InvalidatePlot(false);
            };
        }

        private void PlotEventHandler(OxyInputEventArgs e)
        {
            PlotModel.InvalidatePlot(false);
            e.Handled = true;
        }

        [UsedImplicitly]
        public void CertaintyOptionChosen(object sender, RoutedEventArgs e)
        {
            DialogController.Dialog.ProcessDialog(1);
            _placeholderLine.Points[1] = new DataPoint(DialogController.Dialog.PointToAdd.X, DialogController.Dialog.PointToAdd.Y);
            PlotModel.InvalidatePlot(false);
        }

        [UsedImplicitly]
        public void LotteryOptionChosen(object sender, RoutedEventArgs e)
        {
            DialogController.Dialog.ProcessDialog(2);
            _placeholderLine.Points[1] = new DataPoint(DialogController.Dialog.PointToAdd.X, DialogController.Dialog.PointToAdd.Y);
            PlotModel.InvalidatePlot(false);
        }

        [UsedImplicitly]
        public void IndifferentOptionChosen(object sender, RoutedEventArgs e)
        {
            DialogController.Dialog.ProcessDialog(3);
            GeneratePlotData();
            _calculateUtilities();
        }

        [UsedImplicitly]
        public void ConfirmMethodChoice(object sender, RoutedEventArgs e)
        {
            IsMethodSet = true;
            DialogController = new DialogController(_partialUtility,
                Criterion.MethodOptionsList.IndexOf(Criterion.Method), Criterion.Probability ?? 0);
            InitializePlotIfMethodIsSet();
        }

        [UsedImplicitly]
        public void ResetScalingCoefficients(object sender = null, RoutedEventArgs e = null)
        {
            _restartCoefficientsAssessment();
        }

        [UsedImplicitly]
        public void ResetUtility(object sender = null, RoutedEventArgs e = null)
        {
            RestorePointsValuesToInitialValue();
            DialogController = new DialogController(_partialUtility,
                Criterion.MethodOptionsList.IndexOf(Criterion.Method), Criterion.Probability ?? 0);
            InitializePlotIfMethodIsSet();
        }

        [UsedImplicitly]
        public void ChangeMethodButtonClicked(object sender = null, RoutedEventArgs e = null)
        {
            IsMethodSet = false;
            RestorePointsValuesToInitialValue();
        }

        [UsedImplicitly]
        public void SwitchToSettingExactValue(object sender = null, RoutedEventArgs e = null)
        {
            IsSettingExactValue = true;
        }

        [UsedImplicitly]
        public void SwitchToDialogue(object sender = null, RoutedEventArgs e = null)
        {
            IsSettingExactValue = false;
        }

        private void RestorePointsValuesToInitialValue()
        {
            _partialUtility.PointsValues.Clear();
            foreach (var pointValues in _initialPointsValues)
                _partialUtility.PointsValues.Add(new PartialUtilityValues(pointValues.X, pointValues.Y));
            _calculateUtilities();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}