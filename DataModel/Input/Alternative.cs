using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        public Alternative()
        {
            CriteriaValues = new Dictionary<string, float>();
            CriteriaValuesList = new List<CriterionValue>();
        }

        public Alternative(string name, string description)
        {
            Name = name;
            Description = description;
            CriteriaValues = new Dictionary<string, float>();
            CriteriaValuesList = new List<CriterionValue>();
        }

        private string _name;
        private string _description;
        private Dictionary<string, float> _criteriaValues;

        //todo make sure the order is same as in criteriaList
        public List<CriterionValue> CriteriaValuesList
        {
            //todo onchane event like in CriteriaValues to update everything after adding alternative from datagrid's last row
            get; set;
        }

        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
            Console.WriteLine("Added to alternative " + Name + " value (" + criterionValue.Name + "," + criterionValue.Value + ")");
        }

        public void UpdateCriterionValueValue(string name, string value)
        {
            CriterionValue criterionValue = CriteriaValuesList.Find(c => c.Name == name);
            criterionValue.Name = name;
            criterionValue.Value = value;
            Console.WriteLine("Updated alternative " + Name + ": "+ criterionValue.Name + " set to value: " + value);

            //todo create handler
            OnPropertyChanged("CriteriaValuesList");
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


        /// <summary> pairs: (criterion name, value) </summary>
        public Dictionary<string, float> CriteriaValues
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