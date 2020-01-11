using System;

namespace CalculationsEngine.Assess.Assess
{
    public class LotteriesComparison : Dialog
    {
        public LotteriesComparison(float lowerUtilityBoundary, float upperUtilityBoundary, DisplayObject displayObject) : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
        {

        }

        public override string displayDialog()
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz loterię:");
            Console.WriteLine(DisplayObject.ComparisonLottery.UpperUtilityValue.X + " z prawdopodobienstwem " + DisplayObject.ComparisonLottery.P);
            Console.WriteLine(DisplayObject.ComparisonLottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - DisplayObject.ComparisonLottery.P) + "\n");

            Console.WriteLine("Wpisz '2' jeśli wolisz loterię:");
            Console.WriteLine(DisplayObject.EdgeValuesLottery.UpperUtilityValue.X + " z prawdopodobienstwem " + DisplayObject.EdgeValuesLottery.P);
            Console.WriteLine(DisplayObject.EdgeValuesLottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - DisplayObject.EdgeValuesLottery.P) + "\n");


            Console.WriteLine("Wpisz '3' jeśli loterie są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'1', '2' lub '3' :\n");

            return "";
        }
        public override void setInitialValues()
        {

        }

        protected override void setValuesIfLotteryChosen()
        {
            upperUtilityBoundary = DisplayObject.EdgeValuesLottery.P;
            DisplayObject.EdgeValuesLottery.P = (DisplayObject.EdgeValuesLottery.P + lowerUtilityBoundary) / 2;
        }

        protected override void setValuesIfSureChosen()
        {
            lowerUtilityBoundary = DisplayObject.EdgeValuesLottery.P;
            DisplayObject.EdgeValuesLottery.P = (DisplayObject.EdgeValuesLottery.P + upperUtilityBoundary) / 2;
        }

        protected override void setValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(new Point(DisplayObject.ComparisonLottery.UpperUtilityValue.X, DisplayObject.EdgeValuesLottery.P / DisplayObject.ComparisonLottery.P));
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}
