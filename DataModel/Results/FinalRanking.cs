using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Structs;

namespace DataModel.Results
{
    public class FinalRanking : INotifyPropertyChanged
    {
        private ObservableCollection<FinalRankingEntry> _finalRankingCollection;


        public FinalRanking()
        {
            FinalRankingCollection = new ObservableCollection<FinalRankingEntry>();
        }

        public FinalRanking(ObservableCollection<FinalRankingEntry> finalRankingCollection)
        {
            FinalRankingCollection = finalRankingCollection;
        }


        public ObservableCollection<FinalRankingEntry> FinalRankingCollection
        {
            get => _finalRankingCollection;
            set
            {
                _finalRankingCollection = value;
                OnPropertyChanged(nameof(FinalRankingCollection));
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