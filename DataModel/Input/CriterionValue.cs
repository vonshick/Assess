using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Input
{
    public class CriterionValue : INotifyPropertyChanged
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

        public double? Value
        {
            get => _value;
            set
            {
                if (Nullable.Equals(value, _value)) return;
                if (value == null) throw new ArgumentException("Value is required!");
                // TODO: check if assigning Infinity breaks something, check if decimals work (solver)
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}