using System.ComponentModel;

namespace DataModel.Input
{
    public class CriterionValue : INotifyPropertyChanged
    {
        private float? _value;

        public CriterionValue(string name, float? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public float? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
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
