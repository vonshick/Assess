using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        public Alternative()
        {
            CriteriaValues = new Dictionary<Criterion, float>();
            CriteriaValuesList = new List<CriterionValue>();
            ReferenceRank = -1;
        }

        public void InitCriteriaValues(ObservableCollection<Criterion> criteriaCollection)
        {
            foreach (Criterion criterion in criteriaCollection)
            {
                CriterionValue criterionValue = new CriterionValue(criterion.Name, 2.0f);
                AddCriterionValue(criterionValue);
            }
        }

        public Alternative(string name, string description, ObservableCollection<Criterion> criteriaCollection)
        {
            Name = name;
            Description = description;
            ReferenceRank = -1;
            CriteriaValues = new Dictionary<Criterion, float>();
            CriteriaValuesList = new List<CriterionValue>();
            InitCriteriaValues(criteriaCollection);
        }

        private string _name;
        private string _description;
        private Dictionary<Criterion, float> _criteriaValues;

        //todo make sure the order is same as in criteriaList
        public List<CriterionValue> CriteriaValuesList
        {
            //todo on change event like in CriteriaValues to update everything after adding alternative from datagrid's last row
            get; set;
        }

        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
            Console.WriteLine("Added to alternative " + Name + " value (" + criterionValue.Name + "," + criterionValue.Value + ")");
        }
        
        //todo call it in case of change of any criteria name
        public void UpdateCriterionValueName(string oldName, string newName)
        {
            //call it in criteria class when criteriaList changes. Event should be handled there
            CriterionValue criterionValue = CriteriaValuesList.Find(c => c.Name == oldName);
            criterionValue.Name = newName;
            Console.WriteLine("Updated alternative " + Name + ": set crit name from " + oldName + " to " + newName);
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

        public int ReferenceRank { get; set; }

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
        private void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }
    }
}