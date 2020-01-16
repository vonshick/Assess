using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Results
{
    public class Results : INotifyPropertyChanged
    {
        private double? _k;
        private List<PartialUtility> _partialUtilityFunctions;
        private List<CriterionCoefficient> _criteriaCoefficients;
        public FinalRanking FinalRanking { get; set; }

        public Results()
        {
            FinalRanking = new FinalRanking();
            PartialUtilityFunctions = new List<PartialUtility>();
            CriteriaCoefficients = new List<CriterionCoefficient>();
        }

        public double? K
        {
            get => _k;
            set
            {
                if (Nullable.Equals(value, _k)) return;
                _k = value;
                OnPropertyChanged();
            }
        }

        public List<CriterionCoefficient> CriteriaCoefficients
        {
            get => _criteriaCoefficients;
            set
            {
                _criteriaCoefficients = value;
                OnPropertyChanged(nameof(CriteriaCoefficients));
            }
        }

        public List<PartialUtility> PartialUtilityFunctions
        {
            get => _partialUtilityFunctions;
            set
            {
                _partialUtilityFunctions = value;
                OnPropertyChanged(nameof(PartialUtilityFunctions));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void Reset()
        {
            FinalRanking.FinalRankingCollection.Clear();
            PartialUtilityFunctions.Clear();
            CriteriaCoefficients.Clear();
            K = null;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}