using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;
using DataModel.Input;
using DataModel.Results;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class ReferenceRankingTabViewModel : Tab
    {
        public ReferenceRankingTabViewModel(ReferenceRanking referenceRanking, Alternatives alternatives)
        {
            Name = "Reference Ranking";
            ReferenceRanking = referenceRanking;
            Alternatives = alternatives;

            ReferenceRanking.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(ReferenceRanking.RankingsCollection)) return;
                RefreshFilter();
                InitializeReferenceRankFilterWatchers();
            };

            RefreshFilter();
            InitializeReferenceRankFilterWatchers();

            AlternativesWithoutRanksCollectionView = new ListCollectionView(Alternatives.AlternativesCollection)
            {
                Filter = o => ((Alternative) o).ReferenceRank == null
            };

            NewRank = new ObservableCollection<Alternative>();
            NewRank.CollectionChanged += (sender, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Add) return;
                // NewRank is cleared by AlternativeDroppedOnNewRank event handler, so we always have only one item in this collection
                ReferenceRanking.AddAlternativeToRank(NewRank[0], ReferenceRanking.RankingsCollection.Count);
            };

            RemoveRankCommand = new RelayCommand(rank => ReferenceRanking.RemoveRank((int) rank));
            RemoveAlternativeFromRankCommand = new RelayCommand(alternative =>
            {
                var alternativeToRemove = (Alternative) alternative;
                if (alternativeToRemove.ReferenceRank != null)
                    ReferenceRanking.RemoveAlternativeFromRank(alternativeToRemove, (int) alternativeToRemove.ReferenceRank);
            });
        }


        public ReferenceRanking ReferenceRanking { get; }
        public Alternatives Alternatives { get; }
        public ListCollectionView AlternativesWithoutRanksCollectionView { get; }
        public ObservableCollection<Alternative> NewRank { get; set; }
        public RelayCommand RemoveAlternativeFromRankCommand { get; }
        public RelayCommand RemoveRankCommand { get; }


        [UsedImplicitly]
        public void AddRank()
        {
            ReferenceRanking.AddRank();
        }

        public void InitializeReferenceRankFilterWatchers()
        {
            foreach (var referenceRanking in ReferenceRanking.RankingsCollection)
                referenceRanking.CollectionChanged += InitializeRankFilterWatcher;

            ReferenceRanking.RankingsCollection.CollectionChanged += InitializeRankFilterWatcher;
        }

        private void InitializeRankFilterWatcher(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var addedRank = (ObservableCollection<Alternative>) e.NewItems[0];
                addedRank.CollectionChanged += RefreshFilter;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
            {
                RefreshFilter();
            }
        }

        private void RefreshFilter(object sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            AlternativesWithoutRanksCollectionView.Refresh();
        }
    }
}