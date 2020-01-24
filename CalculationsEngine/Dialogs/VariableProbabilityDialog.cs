using System;
using CalculationsEngine.Maintenance;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class VariableProbabilityDialog : Dialog
    {
        public VariableProbabilityDialog(double lowerUtilityBoundary, double upperUtilityBoundary, DisplayObject displayObject)
            : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
        {
        }

        public override string displayDialog()
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz LOTERIĘ:");
            Console.WriteLine(DisplayObject.Lottery.UpperUtilityValue.X + " z prawdopodobienstwem " + DisplayObject.Lottery.P);
            Console.WriteLine(DisplayObject.Lottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - DisplayObject.Lottery.P) + "\n");
            Console.WriteLine("Wpisz '2' jeśli wolisz równoważnik pewności:");
            Console.WriteLine(DisplayObject.X + "\n");
            Console.WriteLine("Wpisz '3' jeśli loteria i równoważnik pewności są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'1', '2' lub '3' :\n");

            return "";
        }

        protected override void SetInitialValues()
        {
            DisplayObject.X = (LowerUtilityBoundary + UpperUtilityBoundary) / 2;
            PointToAdd = new PartialUtilityValues(DisplayObject.X, DisplayObject.Lottery.NewPointUtility());
        }

        public override void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary)
        {
            LowerUtilityBoundary = lowerUtilityBoundary;
            UpperUtilityBoundary = upperUtilityBoundary;
            SetInitialValues();
        }

        protected override void SetValuesIfLotteryChosen()
        {
            LowerUtilityBoundary = DisplayObject.X;
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfSureChosen()
        {
            UpperUtilityBoundary = DisplayObject.X;
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(PointToAdd);
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}