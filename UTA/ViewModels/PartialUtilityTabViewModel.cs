using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using DataModel.Results;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using UTA.Annotations;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class PartialUtilityTabViewModel : Tab, INotifyPropertyChanged
    {
        private const double AxesExtraSpace = 0.025f;
        private readonly Action _calculateUtilities;
        private readonly OxyColor _colorPrimarySelected = OxyColor.FromArgb(80, 51, 115, 242);
        private readonly OxyColor _colorPrimaryUnselected = OxyColor.FromArgb(30, 51, 115, 242);
        private readonly OxyColor _gridColor = OxyColor.FromRgb(240, 240, 240); // ColorInterface5
        private readonly LineSeries _line;
        private readonly OxyColor _lineColor = OxyColor.FromRgb(110, 110, 110); // ColorSecondary
        private readonly PartialUtility _partialUtility;
        private readonly LineSeries _placeholderLine;
        private readonly OxyColor _placeholderLineColor = OxyColor.FromArgb(70, 110, 110, 110); // ColorSecondary
        private readonly OxyColor _placeholderMarkerColor = OxyColor.FromArgb(170, 51, 115, 242);
        private readonly List<PartialUtilityValues> _pointsValues;
        private readonly SettingsTabViewModel _settings;
        private DialogController _dialogController;
        private bool _isMethodSet;
        private RectangleAnnotation _selectedRectangle;


        public PartialUtilityTabViewModel(PartialUtility partialUtility, Action calculateUtilities)
        {
            _partialUtility = partialUtility;
            _calculateUtilities = calculateUtilities;
            Criterion = partialUtility.Criterion;
            _pointsValues = partialUtility.PointsValues;

            Name = $"Utility - {Criterion.Name}";
            Title = $"{Criterion.Name} - Partial Utility Function";
            IsMethodSet = Criterion.Method != Criterion.MethodOptionsList[0];

            if (IsMethodSet)
            {
                DialogController = new DialogController(_partialUtility,
                    Criterion.MethodOptionsList.IndexOf(Criterion.Method), Criterion.Probability ?? 0);
                DialogController.Dialog.SetInitialValues();
            }
            else
            {
                // choose first method as default, to prevent from not selecting any method at all in radio buttons
                Criterion.Method = Criterion.MethodOptionsList[1];
            }

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
                MarkerFill = _placeholderMarkerColor,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6
            };

            PlotModel = new PlotModel
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
                        AbsoluteMinimum = 0 - AxesExtraSpace,
                        AbsoluteMaximum = 1 + AxesExtraSpace,
                        Minimum = 0 - AxesExtraSpace,
                        Maximum = 1 + AxesExtraSpace,
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
            GeneratePlotData();
            if (IsMethodSet) SelectRectangle((RectangleAnnotation) PlotModel.Annotations[0], 0);
            PlotModel.InvalidatePlot(false);
        }


        // switches ContentControl content for dialogue.
        // nullable because it only switches ContentControl when DisplayObject is initialized and data is bound
        public bool? IsLotteryComparison => DialogController == null ? (bool?) null : Criterion.Method == Criterion.MethodOptionsList[3];
        public IEnumerable<string> Methods { get; } = Criterion.MethodOptionsList.Skip(1);
        public Criterion Criterion { get; }
        public string Title { get; }
        public PlotModel PlotModel { get; }

        public DialogController DialogController
        {
            get => _dialogController;
            set
            {
                _dialogController = value;
                OnPropertyChanged(nameof(DialogController));
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

        public event PropertyChangedEventHandler PropertyChanged;


        public void GeneratePlotData()
        {
            _line.Points.Clear();
            _placeholderLine.Points.Clear();
            PlotModel.Annotations.Clear();
            SelectedRectangle = null;

            var extraSpace = (Criterion.MaxValue - Criterion.MinValue) * AxesExtraSpace;
            PlotModel.Axes[1].AbsoluteMinimum = Criterion.MinValue - extraSpace;
            PlotModel.Axes[1].AbsoluteMaximum = Criterion.MaxValue + extraSpace;
            PlotModel.Axes[1].Minimum = Criterion.MinValue - extraSpace;
            PlotModel.Axes[1].Maximum = Criterion.MaxValue + extraSpace;

            for (var i = 0; i < _pointsValues.Count; i++)
            {
                if (i < _pointsValues.Count - 1)
                {
                    var rectangle = new RectangleAnnotation
                    {
                        MinimumX = _pointsValues[i].X,
                        MaximumX = _pointsValues[i + 1].X,
                        MinimumY = _pointsValues[i].Y,
                        MaximumY = _pointsValues[i + 1].Y,
                        Fill = _colorPrimaryUnselected,
                        Layer = AnnotationLayer.BelowSeries
                    };
                    rectangle.MouseDown += RectangleOnMouseDown;
                    PlotModel.Annotations.Add(rectangle);
                }

                _line.Points.Add(new DataPoint(_pointsValues[i].X, _pointsValues[i].Y));
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
            var firstPointIndex = _pointsValues.FindIndex(partialUtilityValue => partialUtilityValue.X == rectangle.MinimumX);
            DialogController.TriggerDialog(_pointsValues[firstPointIndex], _pointsValues[firstPointIndex + 1]);
            SelectRectangle(rectangle, firstPointIndex);
            PlotEventHandler(e);
        }

        private void SelectRectangle(RectangleAnnotation rectangle, int firstPointIndex)
        {
            if (SelectedRectangle != null) SelectedRectangle.Fill = _colorPrimaryUnselected;
            SelectedRectangle = rectangle;
            rectangle.Fill = _colorPrimarySelected;

            _placeholderLine.Points.Clear();
            _placeholderLine.Points.Add(new DataPoint(_pointsValues[firstPointIndex].X, _pointsValues[firstPointIndex].Y));
            _placeholderLine.Points.Add(new DataPoint(DialogController.Dialog.PointToAdd.X, DialogController.Dialog.PointToAdd.Y));
            _placeholderLine.Points.Add(new DataPoint(_pointsValues[firstPointIndex + 1].X, _pointsValues[firstPointIndex + 1].Y));
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
            DialogController.Dialog.SetInitialValues();
            SelectRectangle((RectangleAnnotation) PlotModel.Annotations[0], 0);
            OnPropertyChanged(nameof(IsLotteryComparison));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}