using System;

namespace CalculationsEngine.Assess.Assess
{
    public class ProbabilityComparison : Dialog
    {
        public ProbabilityComparison(float lowerUtilityBoundary, float upperUtilityBoundary, DisplayObject displayObject) : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
        {
        }

        public override string displayDialog()
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz równoważnik pewności:");
            Console.WriteLine(DisplayObject.X + "\n");
            Console.WriteLine("Wpisz '2' jeśli wolisz LOTERIĘ:");
            Console.WriteLine(DisplayObject.Lottery.UpperUtilityValue.X + " z prawdopodobienstwem " + DisplayObject.Lottery.P);
            Console.WriteLine(DisplayObject.Lottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - DisplayObject.Lottery.P) + "\n");
            Console.WriteLine("Wpisz '3' jeśli loteria i równoważnik pewności są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'1', '2' lub '3' :\n");

            return "";
        }
        protected override void setInitialValues()
        {
            DisplayObject.X = (DisplayObject.Lottery.LowerUtilityValue.X + DisplayObject.Lottery.UpperUtilityValue.X) / 2;
        }

        protected override void setValuesIfLotteryChosen()
        {
            upperUtilityBoundary = DisplayObject.Lottery.P;
            DisplayObject.Lottery.P = (lowerUtilityBoundary + upperUtilityBoundary) / 2;
        }

        protected override void setValuesIfSureChosen()
        {
            lowerUtilityBoundary = DisplayObject.Lottery.P;
            DisplayObject.Lottery.P = (lowerUtilityBoundary + upperUtilityBoundary) / 2;
        }

        protected override void setValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(new Point(DisplayObject.X, DisplayObject.Lottery.NewPointUtility()));
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}
