
namespace CalculationsEngine.Assess.Assess
{
    public class Dialog
    {
        public Dialog(DisplayObject displayObject)
        {
            DisplayObject = displayObject;
        }

        public Dialog(float lowerUtilityBoundary, float upperUtlityBoundary, DisplayObject displayObject)
        {
            this.lowerUtilityBoundary = lowerUtilityBoundary;
            this.upperUtilityBoundary = upperUtlityBoundary;
            DisplayObject = displayObject;
        }

        public DisplayObject DisplayObject;
        protected float lowerUtilityBoundary;
        protected float upperUtilityBoundary;

        protected virtual void setInitialValues()
        {

        }

        protected virtual void setValuesIfLotteryChosen()
        {

        }

        protected virtual void setValuesIfSureChosen()
        {

        }

        protected virtual void setValuesIfEqualChosen()
        {

        }

        public virtual string displayDialog()
        {
            return "";
        }

        public void ProcessDialog(string choice)
        {
            setInitialValues();
//            string choice = displayDialog();

            if (choice.Equals("1"))
            {
                setValuesIfSureChosen();
//                ProcessDialog();
            }
            else if (choice.Equals("2"))
            {
                setValuesIfLotteryChosen();
//                ProcessDialog();
            }
            else if (choice.Equals("3"))
            {
                setValuesIfEqualChosen();
            }
            else
            {
                //TODO vonshick
                // remove the warning - it's useful only for developers
                throw new System.Exception("Assess: wrong choice ID passed to ProcessDialog()");
            }
        }

    }
}
