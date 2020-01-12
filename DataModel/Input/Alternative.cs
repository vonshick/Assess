using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        private ObservableCollection<CriterionValue> _criteriaValuesList;
        private string _name;
        private int? _referenceRank;
        public string ID;


        public Alternative()
        {
            ReferenceRank = null;
            CriteriaValuesList = new ObservableCollection<CriterionValue>();
        }

        public Alternative(string name, IEnumerable<Criterion> criteriaCollection)
        {
            Name = name;
            ReferenceRank = null;
            CriteriaValuesList = new ObservableCollection<CriterionValue>();
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

        public ObservableCollection<CriterionValue> CriteriaValuesList
        {
            get => _criteriaValuesList;
            set
            {
                _criteriaValuesList = value;
                OnPropertyChanged(nameof(CriteriaValuesList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
        }

        public void RemoveCriterionValue(string criterionName)
        {
            var criterionValueToRemove = CriteriaValuesList.First(criterionValue => criterionValue.Name == criterionName);
            CriteriaValuesList.Remove(criterionValueToRemove);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}