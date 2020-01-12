using System;
using System.Collections.Generic;
using System.Windows.Input;
using CalculationsEngine;
using DataModel.Input;
using DataModel.Results;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class ChartTabViewModel : Tab
    {
        private const float AxesExtraSpace = 0.025f;
        private readonly OxyColor _colorPrimary = OxyColor.FromRgb(51, 115, 242); // ColorPrimary
        private readonly Dictionary<float, PointAnnotation> _draggablePoints;
        private readonly TextAnnotation _draggablePointTooltip;
        private readonly OxyColor _draggablePointTooltipFillColor = OxyColor.FromArgb(208, 252, 252, 252); // ColorInterface7
        private readonly OxyColor _draggablePointTooltipStrokeColor = OxyColor.FromRgb(170, 170, 170); // ColorBorders1
        private readonly OxyColor _gridColor = OxyColor.FromRgb(240, 240, 240); // ColorInterface5
        private readonly LineSeries _line;
        private readonly OxyColor _lineColor = OxyColor.FromRgb(110, 110, 110); // ColorSecondary
        private readonly PartialUtility _partialUtility;
        private readonly List<PartialUtilityValues> _pointsValues;
        private readonly Dictionary<float, LineAnnotation> _ranges;
        private readonly OxyColor _rangesColor = OxyColor.FromRgb(210, 210, 210); // ColorInterface2
        private readonly Action _refreshCharts;
        private readonly SettingsTabViewModel _settings;
        private readonly Solver _solver;

        public ChartTabViewModel(Solver solver, PartialUtility partialUtility, SettingsTabViewModel settingsTabViewModel,
            Action refreshCharts)
        {
            _solver = solver;
            _partialUtility = partialUtility;
            Criterion = _partialUtility.Criterion;
            _pointsValues = new List<PartialUtilityValues>(_partialUtility.PointsValues);
            _settings = settingsTabViewModel;
            _refreshCharts = refreshCharts;
            Name = $"Utility - {Criterion.Name}";
            Title = $"{Criterion.Name} - Partial Utility Function";
            _ranges = new Dictionary<float, LineAnnotation>();
            _draggablePoints = new Dictionary<float, PointAnnotation>();

            _line = new LineSeries
            {
                Color = _lineColor,
                StrokeThickness = 3
            };

            _draggablePointTooltip = new TextAnnotation
            {
                Background = _draggablePointTooltipFillColor,
                Stroke = _draggablePointTooltipStrokeColor,
                StrokeThickness = 1,
                Padding = new OxyThickness(8, 2, 8, 2)
            };

            PlotModel = new PlotModel
            {
                Series = {_line},
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
            GenerateChartData();
        }


        public PlotModel PlotModel { get; }
        public Criterion Criterion { get; }
        public string Title { get; }


        public void GenerateChartData()
        {
            _ranges.Clear();
            _draggablePoints.Clear();
            _line.Points.Clear();
            PlotModel.Annotations.Clear();

            var extraSpace = (Criterion.MaxValue - Criterion.MinValue) * AxesExtraSpace;
            PlotModel.Axes[1].AbsoluteMinimum = Criterion.MinValue - extraSpace;
            PlotModel.Axes[1].AbsoluteMaximum = Criterion.MaxValue + extraSpace;
            PlotModel.Axes[1].Minimum = Criterion.MinValue - extraSpace;
            PlotModel.Axes[1].Maximum = Criterion.MaxValue + extraSpace;

            foreach (var pointValues in _pointsValues)
            {
                var point = new DataPoint(pointValues.X, pointValues.Y);
                _line.Points.Add(point);

                var range = new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = pointValues.X,
                    MinimumY = pointValues.MinValue,
                    MaximumY = pointValues.MaxValue,
                    StrokeThickness = 4,
                    Color = _rangesColor,
                    LineStyle = LineStyle.Solid
                };
                _ranges.Add(pointValues.X, range);
                range.MouseDown += AnnotationOnMouseDown;
                range.MouseMove += AnnotationOnMouseMove;
                range.MouseUp += AnnotationOnMouseUp;
                PlotModel.Annotations.Add(range);

                var draggablePoint = new PointAnnotation
                {
                    X = pointValues.X,
                    Y = pointValues.Y,
                    Size = 8,
                    Fill = _colorPrimary
                };
                _draggablePoints.Add(pointValues.X, draggablePoint);
                draggablePoint.MouseDown += AnnotationOnMouseDown;
                draggablePoint.MouseMove += AnnotationOnMouseMove;
                draggablePoint.MouseUp += AnnotationOnMouseUp;
                PlotModel.Annotations.Add(draggablePoint);
            }

            PlotModel.Annotations.Add(_draggablePointTooltip);
            PlotModel.InvalidatePlot(false);
        }

        private void AnnotationOnMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left) return;
            Mouse.OverrideCursor = Cursors.Hand;
            GetLineAndPointAnnotation(sender, out var lineAnnotation, out var pointAnnotation);
            pointAnnotation.Fill = OxyColor.FromRgb(97, 149, 250);
            if (sender is LineAnnotation)
            {
                pointAnnotation.Y = lineAnnotation.InverseTransform(e.Position).Y;
                var linePointIndex = _line.Points.FindIndex(point => point.X == pointAnnotation.X);
                // initializing new datapoint, because it doesn't have a setter
                _line.Points[linePointIndex] = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            }

            GenerateDraggablePointTooltip(pointAnnotation);
            PlotEventHandler(e);
        }

        private void AnnotationOnMouseMove(object sender, OxyMouseEventArgs e)
        {
            GetLineAndPointAnnotation(sender, out var lineAnnotation, out var pointAnnotation);
            var cursorCoords = pointAnnotation.InverseTransform(e.Position);
            if (cursorCoords.Y >= lineAnnotation.MinimumY && cursorCoords.Y <= lineAnnotation.MaximumY)
                pointAnnotation.Y = cursorCoords.Y;
            else if (cursorCoords.Y > lineAnnotation.MaximumY) pointAnnotation.Y = lineAnnotation.MaximumY;
            else pointAnnotation.Y = lineAnnotation.MinimumY;
            var linePointIndex = _line.Points.FindIndex(point => point.X == pointAnnotation.X);
            _line.Points[linePointIndex] = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            GenerateDraggablePointTooltip(pointAnnotation);
            PlotEventHandler(e);
        }

        private void AnnotationOnMouseUp(object sender, OxyMouseEventArgs e)
        {
            GetLineAndPointAnnotation(sender, out _, out var pointAnnotation);
            Mouse.OverrideCursor = null;
            pointAnnotation.Fill = _colorPrimary;
            _draggablePointTooltip.TextPosition = DataPoint.Undefined;
            PlotEventHandler(e);
            //_solver.ChangeValue((float) pointAnnotation.Y, _partialUtility, (float) pointAnnotation.X);
            _refreshCharts();
        }

        private void GenerateDraggablePointTooltip(PointAnnotation pointAnnotation)
        {
            _draggablePointTooltip.Text = Math.Round(pointAnnotation.Y, _settings.PlotsPartialUtilityDecimalPlaces)
                .ToString($"F{_settings.PlotsPartialUtilityDecimalPlaces}");
            _draggablePointTooltip.TextPosition = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            double xOffset = 4 * _settings.PlotsPartialUtilityDecimalPlaces + 24;
            _draggablePointTooltip.Offset = new ScreenVector(pointAnnotation.X == Criterion.MinValue ? xOffset : -1 * xOffset, 9);
        }

        private void GetLineAndPointAnnotation(object sender, out LineAnnotation outLineAnnotation, out PointAnnotation outPointAnnotation)
        {
            if (sender is PointAnnotation)
            {
                outPointAnnotation = (PointAnnotation) sender;
                outLineAnnotation = _ranges[(float) outPointAnnotation.X];
            }
            else
            {
                outLineAnnotation = (LineAnnotation) sender;
                outPointAnnotation = _draggablePoints[(float) outLineAnnotation.X];
            }
        }

        private void PlotEventHandler(OxyInputEventArgs e)
        {
            PlotModel.InvalidatePlot(false);
            e.Handled = true;
        }
    }
}