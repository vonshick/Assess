using System.ComponentModel;
using System.Runtime.CompilerServices;
using UTA.Annotations;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class SettingsTabViewModel : Tab, INotifyPropertyChanged
    {
        // TODO: set defaults later
        private byte _finalRankingUtilityDecimalPlaces = 5;
        private byte _plotsPartialUtilityDecimalPlaces = 3; // TODO: constrain between 1 - 6 inclusive.

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
                _plotsPartialUtilityDecimalPlaces = value;
                OnPropertyChanged(nameof(PlotsPartialUtilityDecimalPlaces));
            }
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


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}