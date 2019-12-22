using DataModel.PropertyChangedExtended;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataModel.Input
{
    public class Criterion : INotifyPropertyChangedExtended<string>
    {
        public Criterion() { }
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

        protected bool Equals(Criterion other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Criterion)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public string ID { get; set; }
        public bool IsEnum { get; set; } = false;
        public Dictionary<string, float> EnumDictionary { get; set; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    string old = _name;
                    _name = value;
                    Console.WriteLine("Crit " + this.GetHashCode() + " has new name: " + value);
                    NotifyPropertyChanged("Name", old, value);
                }
            }
        }

        public string Description { get; set; }
        public enum CriterionDirectionTypes { Gain, Cost, Ordinal };
        public string CriterionDirection { get; set; }
        public int LinearSegments { get; set; }
        // TODO: update min and max values after value changes in alternative editor
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }
        protected void NotifyPropertyChanged(string propertyName, string oldValue, string newValue)
        {
            OnPropertyChanged(this, new PropertyChangedExtendedEventArgs<string>(propertyName, oldValue, newValue));
        }

    }
}
