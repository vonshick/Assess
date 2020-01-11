using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.PropertyChangedExtended;

namespace DataModel.Input
{
    public class Criterion : INotifyPropertyChanged, INotifyPropertyChangedExtended<string>
    {
        public static double MinNumberOfLinearSegments = 1; // type double, because can't use other type in xaml
        public static double MaxNumberOfLinearSegments = 99; // TODO: change this value according to app performance
        private string _criterionDirection;
        private string _description;
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

        [UsedImplicitly] public static string[] CriterionDirectionTypesList { get; } = {"Gain", "Cost"};

        public string ID { get; set; }
        public float MinValue { get; set; } = float.MaxValue;
        public float MaxValue { get; set; } = float.MinValue;
        public bool IsEnum { get; set; } = false;
        public Dictionary<string, float> EnumDictionary { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                var oldValue = _name;
                _name = value;
                OnPropertyChangedExtended(nameof(Name), oldValue, value);
            }
        }

        public string CriterionDirection
        {
            get => _criterionDirection;
            set
            {
                if (value == _criterionDirection) return;
                if (value != "Cost" && value != "Gain")
                    throw new ArgumentException("Value must be \"Cost\" or \"Gain\".");
                _criterionDirection = value;
                OnPropertyChanged(nameof(CriterionDirection));
            }
        }

        public int LinearSegments
        {
            get => _linearSegments;
            set
            {
                if (value == _linearSegments) return;
                if (value < MinNumberOfLinearSegments || value > MaxNumberOfLinearSegments)
                    throw new ArgumentException(
                        $"Value must be between {MinNumberOfLinearSegments} - {MaxNumberOfLinearSegments} inclusive.");
                _linearSegments = value;
                OnPropertyChanged(nameof(LinearSegments));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChangedExtended(string propertyName, string oldValue, string newValue)
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<string>(propertyName, oldValue, newValue));
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