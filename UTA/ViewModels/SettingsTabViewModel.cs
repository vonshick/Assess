using System.ComponentModel;
using System.Runtime.CompilerServices;
using UTA.Annotations;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class SettingsTabViewModel : Tab, INotifyPropertyChanged
    {
        private byte _chartsAlternativesValuesMaxDecimalPlaces = 15; // TODO: constrain between 0 - 6 inclusive
        private byte _finalRankingUtilityDecimalPlaces = 5;


        public SettingsTabViewModel()
        {
            Name = "Settings";
        }


        public byte FinalRankingUtilityDecimalPlaces
        {
            get => _finalRankingUtilityDecimalPlaces;
            set
            {
                if (value == _finalRankingUtilityDecimalPlaces) return;
                _finalRankingUtilityDecimalPlaces = value;
                OnPropertyChanged(nameof(FinalRankingUtilityDecimalPlaces));
            }
        }

        public byte ChartsAlternativesValuesMaxDecimalPlaces
        {
            get => _chartsAlternativesValuesMaxDecimalPlaces;
            set
            {
                if (value == _chartsAlternativesValuesMaxDecimalPlaces) return;
                _chartsAlternativesValuesMaxDecimalPlaces = value;
                OnPropertyChanged(nameof(ChartsAlternativesValuesMaxDecimalPlaces));
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