using System;
using System.Collections.Generic;
using System.ComponentModel;
using DataModel.PropertyChangedExtended;

namespace DataModel.Input
{
    public class Criterion : INotifyPropertyChangedExtended<string>, INotifyPropertyChanged
    {
        public enum CriterionDirectionTypes
        {
            Gain,
            Cost
        }

        public static double MinNumberOfLinearSegments = 1; // type double, because can't use other type in xaml
        public static double MaxNumberOfLinearSegments = 99;
        private int _linearSegments;
        private string _name;

        public Criterion()
        {
        }

        public Criterion(string name, string criterionDirection)
        {
            Name = name;
            CriterionDirection = criterionDirection;
        }

        public Criterion(string name, string description, string criterionDirection, int linearSegments)
        {
            Name = name;
            Description = description;
            CriterionDirection = criterionDirection;
            LinearSegments = linearSegments;
        }

        public string ID { get; set; }
        public bool IsEnum { get; set; } = false;
        public Dictionary<string, float> EnumDictionary { get; set; }
        public string Description { get; set; }
        public string CriterionDirection { get; set; }
        // TODO: update min and max values after value changes in alternative editor
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                var oldValue = _name;
                _name = value;
                Console.WriteLine("Crit " + GetHashCode() + " has new name: " + value);
                OnPropertyChangedExtended("Name", oldValue, value);
            }
        }

        public int LinearSegments
        {
            get => _linearSegments;
            set
            {
                if (value == _linearSegments) return;
                _linearSegments = value;
                OnPropertyChanged(nameof(LinearSegments));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Equals(Criterion other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Criterion) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChangedExtended(string propertyName, string oldValue, string newValue)
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<string>(propertyName, oldValue, newValue));
        }
    }
}