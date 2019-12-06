using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        public Alternative()
        {
        }

        public Alternative(string name, string description)
        {
            Name = name;
            Description = description;
        }

        private string _name;
        private string _description;
        private Dictionary<string, float> _criteriaValues;

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