using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DisplayObject : INotifyPropertyChanged
    {
        private double _p;
        public Lottery ComparisonLottery; //lotteries comparison
        public Lottery EdgeValuesLottery; //lotteries comparison
        public Lottery Lottery;
        public List<PartialUtilityValues> PointsList;

        public DisplayObject()
        {
            PointsList = new List<PartialUtilityValues>();
        }

        // used in CoefficientsDialog
        public DisplayObject(List<Criterion> criterionList, double[] valuesToCompare, double p)
        {
            P = p;
            CoefficientsDialogValuesList = new List<CoefficientsDialogValues>();
            for (var i = 0; i < criterionList.Count; i++)
            {
                CoefficientsDialogValuesList.Add(new CoefficientsDialogValues(
                    valuesToCompare[i],
                    criterionList[i].MaxValue,
                    criterionList[i].MinValue,
                    criterionList[i].Name
                ));
            }
        }

        public List<CoefficientsDialogValues> CoefficientsDialogValuesList { get; set; }
        public double X { get; set; } //partial probability assessment dialog
        public double P //lotteries comparison dialog, constant probability dialog, coefficients dialog (starts with 0.5 by default)
        {
            get => _p;
            set
            {
                if (value.Equals(_p)) return;
                _p = value;
                OnPropertyChanged(nameof(P));
                OnPropertyChanged(nameof(ComplementaryP));
            }
        }

        public double ComplementaryP => 1 - P;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}