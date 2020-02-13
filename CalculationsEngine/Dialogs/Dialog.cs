using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Maintenance;
using DataModel.Annotations;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class Dialog : INotifyPropertyChanged
    {
        private PartialUtilityValues _pointToAdd;
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


        public PartialUtilityValues PointToAdd
        {
            get => _pointToAdd;
            set
            {
                if (Equals(value, _pointToAdd)) return;
                _pointToAdd = value;
                OnPropertyChanged(nameof(PointToAdd));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public virtual void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary)
        {
        }

        protected virtual void SetInitialValues()
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


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}