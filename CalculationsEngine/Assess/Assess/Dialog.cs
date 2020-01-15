using System;

namespace CalculationsEngine.Assess.Assess
{
    public class Dialog
    {
        public DisplayObject DisplayObject;
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

        protected virtual void SetValuesIfLotteryChosen()
        {
        }

        protected virtual void SetValuesIfSureChosen()
        {
        }

        protected virtual void SetValuesIfEqualChosen()
        {
        }

        //todo remove
        public virtual string displayDialog()
        {
            return "";
        }

        public void ProcessDialog(int choice)
        {
            if (choice == 1)
                SetValuesIfSureChosen();
            else if (choice == 2)
                SetValuesIfLotteryChosen();
            else if (choice == 3)
                SetValuesIfEqualChosen();
            else
                //TODO vonshick
                // remove the warning - it's useful only for developers
                throw new Exception("Assess: wrong choice ID passed to ProcessDialog()");
            SetInitialValues();
        }
    }
}