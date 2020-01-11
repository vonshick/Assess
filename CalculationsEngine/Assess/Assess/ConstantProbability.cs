using System;

namespace CalculationsEngine.Assess.Assess
{
    public class ConstantProbability : Dialog
    {
        public ConstantProbability(float lowerUtilityBoundary, float upperUtilityBoundary, DisplayObject displayObject) : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
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

//            string choice = Console.ReadLine();

            return "";
        }

        public override void setInitialValues()
        {
            DisplayObject.X = lowerUtilityBoundary + (upperUtilityBoundary - lowerUtilityBoundary) * DisplayObject.Lottery.P;
        }

        protected override void setValuesIfLotteryChosen()
        {
            lowerUtilityBoundary = DisplayObject.X;
        }

        protected override void setValuesIfSureChosen()
        {
            upperUtilityBoundary = DisplayObject.X;
        }

        protected override void setValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(new Point(DisplayObject.X, DisplayObject.Lottery.NewPointUtility()));
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}
