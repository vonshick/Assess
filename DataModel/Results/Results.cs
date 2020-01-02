using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Results
{
    //very general class concept for now
    public class Results : INotifyPropertyChanged
    {
        private List<PartialUtility> _partialUtilityFunctions;

        public Results()
        {
            FinalRanking = new FinalRanking();
            PartialUtilityFunctions = new List<PartialUtility>();
        }


        public FinalRanking FinalRanking { get; set; }

        public List<PartialUtility> PartialUtilityFunctions
        {
            get => _partialUtilityFunctions;
            set
            {
                if (Equals(value, _partialUtilityFunctions)) return;
                _partialUtilityFunctions = value;
                OnPropertyChanged(nameof(PartialUtilityFunctions));
            }
        }

        public float KendallCoefficient { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public void Reset()
        {
            FinalRanking.FinalRankingCollection.Clear();
            PartialUtilityFunctions.Clear();
            KendallCoefficient = 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}