using System;
using System.Collections.Generic;
using System.Windows.Input;
using DataModel.Input;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class ChartTabViewModel : Tab
    {
        private readonly Criterion _criterion;
        private readonly Alternatives _alternatives;
        private readonly SettingsTabViewModel _settings;
        private const float AxesExtraSpace = 0.02f;

        private readonly OxyColor _colorPrimary = OxyColor.FromRgb(51, 115, 242);
        private LineSeries _line;
        private readonly Dictionary<float, LineAnnotation> _lineAnnotations;
        private readonly Dictionary<float, PointAnnotation> _pointAnnotations;
        private readonly Dictionary<float, DataPoint> _points;

        public ChartTabViewModel(Criterion criterion, Alternatives alternatives, SettingsTabViewModel settingsTabViewModel)
        {
            Name = $"Utility - {criterion.Name}";
            _criterion = criterion;
            _alternatives = alternatives;
            _settings = settingsTabViewModel;
            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ChartsAlternativesValuesMaxDecimalPlaces") GenerateChartData();
            };

            _line = new LineSeries
            {
                Color = OxyColor.FromRgb(90, 90, 90),
                LineStyle = LineStyle.Solid,
                StrokeThickness = 3
            };
            PlotModel = new PlotModel
            {
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
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = OxyColor.FromRgb(240,240,240),
                        AbsoluteMinimum = 0 - AxesExtraSpace,
                        AbsoluteMaximum = 1 + AxesExtraSpace,
                        Minimum = 0 - AxesExtraSpace,
                        Maximum = 1 + AxesExtraSpace,
                        MajorTickSize = 8,
                        IntervalLength = 30,
                        AxisTitleDistance = 12,
                        FontSize = 16
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        Title = "Criterion Value",
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = OxyColor.FromRgb(240,240,240),
                        MajorTickSize = 8,
                        AxisTitleDistance = 4,
                        FontSize = 16
                    }
                },
                Series = {_line}
            };

            _lineAnnotations = new Dictionary<float, LineAnnotation>();
            _pointAnnotations = new Dictionary<float, PointAnnotation>();
            _points = new Dictionary<float, DataPoint>();
            GenerateChartData();
        }


        public PlotModel PlotModel { get; }

        // TODO: check if it's working on alternatives, criteria update
        private void GenerateChartData()
        {
            _lineAnnotations.Clear();
            _pointAnnotations.Clear();
            _points.Clear();
            var extraSpace = (_criterion.MaxValue - _criterion.MinValue) * AxesExtraSpace;
            PlotModel.Axes[1].AbsoluteMinimum = _criterion.MinValue - extraSpace;
            PlotModel.Axes[1].AbsoluteMaximum = _criterion.MaxValue + extraSpace;
            PlotModel.Axes[1].Minimum = _criterion.MinValue - extraSpace;
            PlotModel.Axes[1].Maximum = _criterion.MaxValue + extraSpace;
            var xCoords = Linspace(_criterion.MinValue, _criterion.MaxValue, _criterion.LinearSegments);
            // TODO: run solver here. get initial partial utilities for xCoords from solver.
            for (var i = 0; i < xCoords.Length; i++)
            {
                var x = xCoords[i];
                var y = xCoords[i] * 1f;
                // TODO: get second datapoint value from solver
                var point = new DataPoint(x, y);
                _points.Add(x, point);
                _line.Points.Add(point);

                var lineAnnotation = new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = x,
                    MinimumY = y - 0.1, // TODO: get solver value here
                    MaximumY = y + 0.3, // TODO: get solver value here
                    StrokeThickness = 4,
                    Color = OxyColor.FromRgb(210, 210, 210),
                    LineStyle = LineStyle.Solid
                };
                _lineAnnotations.Add(x, lineAnnotation);
                lineAnnotation.MouseDown += AnnotationOnMouseDown;
                lineAnnotation.MouseMove += AnnotationOnMouseMove;
                lineAnnotation.MouseUp += AnnotationOnMouseUp;
                PlotModel.Annotations.Add(lineAnnotation);

                var pointAnnotation = new PointAnnotation
                {
                    X = x,
                    Y = y,
                    Size = 8,
                    Fill = _colorPrimary
                };
                _pointAnnotations.Add(x, pointAnnotation);
                pointAnnotation.MouseDown += AnnotationOnMouseDown;
                pointAnnotation.MouseMove += AnnotationOnMouseMove;
                pointAnnotation.MouseUp += AnnotationOnMouseUp;
                PlotModel.Annotations.Add(pointAnnotation);
            }
            PlotModel.InvalidatePlot(false);
        }

        private float[] Linspace(float start, float stop, int num)
        {
            num += 1;
            var linspace = new float[num];
            var step = (stop - start) / (num - 1);
            for (var i = 0; i < num; i++) linspace[i] = (float) Math.Round(step * i, _settings.ChartsAlternativesValuesMaxDecimalPlaces);
            return linspace;
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
                var index = _line.Points.FindIndex(point => point.X == pointAnnotation.X);
                _line.Points[index] = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            }

            PlotEventHandler(e);
        }

        private void AnnotationOnMouseMove(object sender, OxyMouseEventArgs e)
        {
            GetLineAndPointAnnotation(sender, out var lineAnnotation, out var pointAnnotation);
            var currentCoords = pointAnnotation.InverseTransform(e.Position);
            if (currentCoords.Y >= lineAnnotation.MinimumY && currentCoords.Y <= lineAnnotation.MaximumY)
                pointAnnotation.Y = currentCoords.Y;
            else if (currentCoords.Y > lineAnnotation.MaximumY) pointAnnotation.Y = lineAnnotation.MaximumY;
            else if (currentCoords.Y < lineAnnotation.MinimumY) pointAnnotation.Y = lineAnnotation.MinimumY;
            else return;
            var index = _line.Points.FindIndex(point => point.X == pointAnnotation.X);
            _line.Points[index] = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            PlotEventHandler(e);
        }

        private void AnnotationOnMouseUp(object sender, OxyMouseEventArgs e)
        {
            // TODO: run solver
            GetLineAndPointAnnotation(sender, out _, out var pointAnnotation);
            Mouse.OverrideCursor = null;
            pointAnnotation.Fill = _colorPrimary;
            PlotEventHandler(e);
        }

        private void GetLineAndPointAnnotation(object sender, out LineAnnotation outLineAnnotation, out PointAnnotation outPointAnnotation)
        {
            if (sender is PointAnnotation)
            {
                outPointAnnotation = (PointAnnotation) sender;
                outLineAnnotation = _lineAnnotations[(float) outPointAnnotation.X];
            }
            else
            {
                outLineAnnotation = (LineAnnotation) sender;
                outPointAnnotation = _pointAnnotations[(float) outLineAnnotation.X];
            }
        }

        private void PlotEventHandler(OxyInputEventArgs e)
        {
            PlotModel.InvalidatePlot(false);
            e.Handled = true;
        }
    }
}