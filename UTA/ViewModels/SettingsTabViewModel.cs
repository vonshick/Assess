using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UTA.Annotations;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class SettingsTabViewModel : Tab, INotifyPropertyChanged
    {
        // TODO: set defaults later
        private byte _plotsPartialUtilityDecimalPlaces = 3;
        private double _deltaThreshold = 0.05;
        private double _epsilonThreshold = 0.0000001;

        public SettingsTabViewModel()
        {
            Name = "Settings";
        }

        public byte PlotsPartialUtilityDecimalPlaces
        {
            get => _plotsPartialUtilityDecimalPlaces;
            set
            {
                if (value == _plotsPartialUtilityDecimalPlaces) return;
                if (value < 1 || value > 7) throw new ArgumentException("Value must be between 1 - 7 inclusive.");
                _plotsPartialUtilityDecimalPlaces = value;
                OnPropertyChanged(nameof(PlotsPartialUtilityDecimalPlaces));
            }
        }

        public double DeltaThreshold
        {
            get => _deltaThreshold;
            set
            {
                if (value.Equals(_deltaThreshold)) return;
                if (value < 0 || value > 1) throw new ArgumentException("Value must be between 0 - 1 inclusive.");
                _deltaThreshold = value;
                OnPropertyChanged(nameof(DeltaThreshold));
            }
        }

        public double EpsilonThreshold
        {
            get => _epsilonThreshold;
            set
            {
                if (value.Equals(_epsilonThreshold)) return;
                if (value < 0 || value > 1) throw new ArgumentException("Value must be between 0 - 1 inclusive.");
                _epsilonThreshold = value;
                OnPropertyChanged(nameof(EpsilonThreshold));
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