using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        private string _name;
        private int? _referenceRank;
        public string ID;

        public Alternative()
        {
            ReferenceRank = null;
            CriteriaValuesList = new List<CriterionValue>();
        }

        public Alternative(string name, IEnumerable<Criterion> criteriaCollection)
        {
            Name = name;
            ReferenceRank = null;
            CriteriaValuesList = new List<CriterionValue>();
            foreach (var criterion in criteriaCollection) AddCriterionValue(new CriterionValue(criterion.Name, null));
        }


        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int? ReferenceRank
        {
            get => _referenceRank;
            set
            {
                if (_referenceRank == value) return;
                _referenceRank = value;
                OnPropertyChanged(nameof(ReferenceRank));
            }
        }

        public List<CriterionValue> CriteriaValuesList { get; set; }

        //TODO remove, CriteriaValuesList used now instead
        public Dictionary<Criterion, float> CriteriaValues { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
        }

        public void RemoveCriterionValue(string criterionName)
        {
            CriteriaValuesList.RemoveAll(criterionValue => criterionValue.Name == criterionName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}