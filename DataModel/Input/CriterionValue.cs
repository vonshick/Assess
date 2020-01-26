using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.PropertyChangedExtended;

namespace DataModel.Input
{
    public class CriterionValue : INotifyPropertyChanged, INotifyPropertyChangedExtended<double?>
    {
        private string _name;
        private double? _value;


        public CriterionValue(string name, double? value)
        {
            Name = name;
            Value = value;
        }


        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        // null is acceptable only during initialization
        public double? Value
        {
            get => _value;
            set
            {
                if (Nullable.Equals(value, _value)) return;
                if (value == null) throw new ArgumentException("Value is required!");
                var oldValue = _value;
                _value = value;
                OnPropertyChangedExtended(nameof(Value), oldValue, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChangedExtended(string propertyName, double? oldValue, double? newValue)
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<double?>(propertyName, oldValue, newValue));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}