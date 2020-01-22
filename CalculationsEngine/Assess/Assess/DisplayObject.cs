using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DisplayObject : INotifyPropertyChanged
    {
        private Lottery _comparisonLottery;
        private Lottery _edgeValuesLottery;
        private Lottery _lottery;
        private double _p;
        private double _x;
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
                CoefficientsDialogValuesList.Add(new CoefficientsDialogValues(
                    valuesToCompare[i],
                    criterionList[i].MaxValue,
                    criterionList[i].MinValue,
                    criterionList[i].Name
                ));
        }


        public Lottery Lottery
        {
            get => _lottery;
            set
            {
                if (Equals(value, _lottery)) return;
                _lottery = value;
                OnPropertyChanged(nameof(Lottery));
            }
        }

        public double X
        {
            get => _x;
            set
            {
                if (value.Equals(_x)) return;
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

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

        public Lottery ComparisonLottery //lotteries comparison
        {
            get => _comparisonLottery;
            set
            {
                _comparisonLottery = value;
                OnPropertyChanged(nameof(ComparisonLottery));
            }
        }

        public Lottery EdgeValuesLottery //lotteries comparison
        {
            get => _edgeValuesLottery;
            set
            {
                _edgeValuesLottery = value;
                OnPropertyChanged(nameof(EdgeValuesLottery));
            }
        }

        public List<CoefficientsDialogValues> CoefficientsDialogValuesList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}