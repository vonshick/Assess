using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Results;

namespace CalculationsEngine.Models
{
    public class Lottery : INotifyPropertyChanged
    {
        private double _p;


        public Lottery(PartialUtilityValues lowerUtilityValue, PartialUtilityValues upperUtilityValue)
        {
            LowerUtilityValue = lowerUtilityValue;
            UpperUtilityValue = upperUtilityValue;
        }


        public double P
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
        public PartialUtilityValues LowerUtilityValue { get; set; }
        public PartialUtilityValues UpperUtilityValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public void SetProbability(double p)
        {
            P = p;
        }

        public double NewPointUtility()
        {
            return P * UpperUtilityValue.Y + (1 - P) * LowerUtilityValue.Y;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}