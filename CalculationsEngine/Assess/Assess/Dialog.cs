using System;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class Dialog
    {
        public DisplayObject DisplayObject;
        public PartialUtilityValues PointToAdd;
        protected double LowerUtilityBoundary;
        protected double UpperUtilityBoundary;

        public Dialog(DisplayObject displayObject)
        {
            DisplayObject = displayObject;
        }

        public Dialog(double lowerUtilityBoundary, double upperUtilityBoundary, DisplayObject displayObject)
        {
            LowerUtilityBoundary = lowerUtilityBoundary;
            UpperUtilityBoundary = upperUtilityBoundary;
            DisplayObject = displayObject;
        }

        protected Dialog()
        {
        }

        public virtual void SetInitialValues()
        {
        }

        public virtual void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary)
        {
        }

        protected virtual void SetValuesIfLotteryChosen()
        {
        }

        protected virtual void SetValuesIfSureChosen()
        {
        }

        protected virtual void SetValuesIfEqualChosen()
        {
        }

        protected void UpdateLotteryComparisonPointToAdd()
        {
            PointToAdd.X = DisplayObject.ComparisonLottery.UpperUtilityValue.X;
            PointToAdd.Y = DisplayObject.EdgeValuesLottery.P / DisplayObject.ComparisonLottery.P;
        }

        protected void UpdateOtherMethodsPointToAdd()
        {
            PointToAdd.X = DisplayObject.X;
            PointToAdd.Y = DisplayObject.Lottery.NewPointUtility();
        }

        //todo remove
        public virtual string displayDialog()
        {
            return "";
        }

        public void ProcessDialog(int choice)
        {
            if (choice == 1)
            {
                SetValuesIfSureChosen();
                SetInitialValues();
            }
            else if (choice == 2)
            {
                SetValuesIfLotteryChosen();
                SetInitialValues();
            }
            else if (choice == 3)
            {
                SetValuesIfEqualChosen();
            }
        }
    }
}