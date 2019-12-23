using System;
using System.Collections.Generic;
using DataModel.Input;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class ChartTabViewModel : Tab
    {
        private readonly OxyColor _colorPrimary = OxyColor.FromRgb(51, 115, 242);
        private readonly LineSeries _line;
        private readonly Dictionary<float, LineAnnotation> _lineAnnotations;
        private readonly Dictionary<float, PointAnnotation> _pointAnnotations;
        private readonly Dictionary<float, DataPoint> _points;

        public ChartTabViewModel(Criterion criterion, Alternatives alternatives)
        {
            Name = $"Utility - {criterion.Name}";
            Criterion = criterion;
            Alternatives = alternatives;

            _line = new LineSeries
            {
                Color = OxyColor.FromRgb(90, 90, 90)
            };
            PlotModel = new PlotModel {Series = {_line}};

            _lineAnnotations = new Dictionary<float, LineAnnotation>();
            _pointAnnotations = new Dictionary<float, PointAnnotation>();
            _points = new Dictionary<float, DataPoint>();
            // TODO: get min max values from Criterion
            //var xCoords = Linspace(criterion.MinValue, criterion.MaxValue, criterion.LinearSegments);
            var xCoords = Linspace(0, 100, criterion.LinearSegments);
            // TODO: run solver here. get initial partial utilities for lineBreaksXCoordinates from solver.
            for (var i = 0; i < xCoords.Length; i++)
            {
                var x = xCoords[i];
                var y = x * i;
                // TODO: get second datapoint value from solver
                var point = new DataPoint(x, y);
                _points.Add(x, point);
                _line.Points.Add(point);

                var lineAnnotation = new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = x,
                    MinimumY = y - 20, // TODO: get solver value here
                    MaximumY = y + 40, // TODO: get solver value here
                    StrokeThickness = 3,
                    Color = OxyColor.FromRgb(210, 210, 210),
                    LineStyle = LineStyle.Solid,
                    Selectable = false
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
        }

        private Criterion Criterion { get; }
        private Alternatives Alternatives { get; }
        public PlotModel PlotModel { get; }


        private void AnnotationOnMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left) return;
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
            // TODO: change cursor to grip hand
            GetLineAndPointAnnotation(sender, out var lineAnnotation, out var pointAnnotation);
            var currentCoords = pointAnnotation.InverseTransform(e.Position);
            if (currentCoords.Y >= lineAnnotation.MinimumY && currentCoords.Y <= lineAnnotation.MaximumY)
                pointAnnotation.Y = currentCoords.Y;
            else if (currentCoords.Y > lineAnnotation.MaximumY) pointAnnotation.Y = lineAnnotation.MaximumY;
            else if (currentCoords.Y < lineAnnotation.MinimumY) pointAnnotation.Y = lineAnnotation.MinimumY;
            var index = _line.Points.FindIndex(point => point.X == pointAnnotation.X);
            _line.Points[index] = new DataPoint(pointAnnotation.X, pointAnnotation.Y);
            PlotEventHandler(e);
        }

        private void AnnotationOnMouseUp(object sender, OxyMouseEventArgs e)
        {
            // TODO: run solver
            GetLineAndPointAnnotation(sender, out _, out var pointAnnotation);
            pointAnnotation.Fill = _colorPrimary;
            PlotEventHandler(e);
        }

        private void GetLineAndPointAnnotation(object sender, out LineAnnotation outLineAnnotation,
            out PointAnnotation outPointAnnotation)
        {
            if (sender is PointAnnotation pointAnnotation)
            {
                outPointAnnotation = pointAnnotation;
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

        private float[] Linspace(float start, float stop, int num)
        {
            var linspace = new float[num];
            var step = (stop - start) / num;
            for (var i = 0; i < num; i++) linspace[i] = (float) Math.Round(step * i, 5);
            return linspace;
        }
    }
}