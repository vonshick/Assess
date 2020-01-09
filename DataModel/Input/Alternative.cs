using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        private Dictionary<Criterion, float> _criteriaValues;
        private string _description;
        private string _name;
        private int? _referenceRank;
        public string ID;

        public Alternative()
        {
            CriteriaValues = new Dictionary<Criterion, float>();
            CriteriaValuesList = new List<CriterionValue>();
            ReferenceRank = null;
        }

        public Alternative(string name, string description, ObservableCollection<Criterion> criteriaCollection)
        {
            Name = name;
            Description = description;
            ReferenceRank = null;
            CriteriaValues = new Dictionary<Criterion, float>();
            CriteriaValuesList = new List<CriterionValue>();
            InitCriteriaValues(criteriaCollection);
        }


        //todo make sure the order is same as in criteriaList
        public List<CriterionValue> CriteriaValuesList
        {
            //todo on change event like in CriteriaValues to update everything after adding alternative from datagrid's last row
            get;
            set;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public int? ReferenceRank
        {
            get => _referenceRank;
            set
            {
                if (_referenceRank != value)
                {
                    _referenceRank = value;
                    OnPropertyChanged("ReferenceRank");
                }
            }
        }

        //TODO remove, CriteriaValuesList used now instead
        /// <summary> pairs: (criterion name, value) </summary>
        public Dictionary<Criterion, float> CriteriaValues
        {
            get => _criteriaValues;
            set
            {
                if (_criteriaValues != value)
                {
                    _criteriaValues = value;
                    OnPropertyChanged("CriteriaValues");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        

        public void InitCriteriaValues(ObservableCollection<Criterion> criteriaCollection)
        {
            foreach (var criterion in criteriaCollection)
            {
                var criterionValue = new CriterionValue(criterion.Name, null);
                AddCriterionValue(criterionValue);
            }
        }

        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
        }

        public void UpdateCriterionValueName(string oldName, string newName)
        {
            var criterionValue = CriteriaValuesList.Find(c => c.Name == oldName);
            criterionValue.Name = newName;
        }

        private void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}