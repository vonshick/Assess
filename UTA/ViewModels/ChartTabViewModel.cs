using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ControlzEx.Standard;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
   public class ChartTabViewModel : Tab
   {
      public ChartTabViewModel(Criteria criteria, Alternatives alternatives)
      {
         Name = "Chart";
         Criteria = criteria;
         Alternatives = alternatives;


         this.Title = "Example 2";
         this.Points = new List<DataPoint>
         {
            new DataPoint(0, 4),
            new DataPoint(10, 13),
            new DataPoint(20, 15),
            new DataPoint(30, 16),
            new DataPoint(40, 12),
            new DataPoint(50, 12)
         };

         Line = new LineSeries();
         Points.ForEach(p =>
         {
            Line.Points.Add(p);
         });
         Line.MarkerType = MarkerType.Circle;
         Line.MarkerFill = OxyColors.Red;

         this.MyModel = new PlotModel { Title = "Example 1" };
         MyModel.Series.Add(Line);
         
         var ann = new PointAnnotation() {X = 10, Y = 10, StrokeThickness = 5};
         ann.MouseDown += AnnOnMouseDown;
         MyModel.Annotations.Add(ann);
      }

      private void AnnOnMouseDown(object sender, OxyMouseDownEventArgs e)
      {
         var a = ((PointAnnotation)sender).InverseTransform(e.Position);
      }


      private Criteria Criteria { get; }
      private Alternatives Alternatives { get; }


      public string Title { get; private set; }

      public PlotModel MyModel { get; private set; }
      public LineSeries Line { get; set; }
      public List<DataPoint> Points { get; set; }
   }
}