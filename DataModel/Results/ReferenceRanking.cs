using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Input;

namespace DataModel.Results
{
    public class ReferenceRanking : INotifyPropertyChanged
    {
        private ObservableCollection<ObservableCollection<Alternative>> _rankingsCollection;


        public ReferenceRanking()
        {
            RankingsCollection = new ObservableCollection<ObservableCollection<Alternative>>();
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(RankingsCollection)) return;
                InitializeAlternativeReferenceRankUpdaterWatcher();
            };

            InitializeAlternativeReferenceRankUpdaterWatcher();
        }


        public ObservableCollection<ObservableCollection<Alternative>> RankingsCollection
        {
            get => _rankingsCollection;
            set
            {
                _rankingsCollection = value;
                OnPropertyChanged(nameof(RankingsCollection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void InitializeAlternativeReferenceRankUpdaterWatcher()
        {
            foreach (var referenceRanking in RankingsCollection)
                referenceRanking.CollectionChanged += UpdateAlternativeReferenceRankingIndex;

            RankingsCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedRank = (ObservableCollection<Alternative>) args.NewItems[0];
                    addedRank.CollectionChanged += UpdateAlternativeReferenceRankingIndex;
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    var removedRank = (ObservableCollection<Alternative>) args.OldItems[0];
                    foreach (var alternative in removedRank)
                        alternative.ReferenceRank = null;

                    for (var i = args.OldStartingIndex; i < RankingsCollection.Count; i++)
                        foreach (var alternative in RankingsCollection[i])
                            alternative.ReferenceRank = i;
                }
            };
        }

        private void UpdateAlternativeReferenceRankingIndex(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var rank = (ObservableCollection<Alternative>) sender;
                var rankIndex = RankingsCollection.IndexOf(rank);
                var addedAlternative = (Alternative) e.NewItems[0];
                addedAlternative.ReferenceRank = rankIndex;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removedAlternative = (Alternative) e.OldItems[0];
                removedAlternative.ReferenceRank = null;
            }
        }

        public void AddAlternativeToRank(Alternative alternative, int rank)
        {
            if (RankingsCollection.Count <= rank)
                while (RankingsCollection.Count <= rank)
                    AddRank();
            RankingsCollection[rank].Add(alternative);
        }

        public void RemoveAlternativeFromRank(Alternative alternative, int rank)
        {
            RankingsCollection[rank].Remove(alternative);
        }

        public void AddRank()
        {
            RankingsCollection.Add(new ObservableCollection<Alternative>());
        }

        public void RemoveRank(int rank)
        {
            RankingsCollection.RemoveAt(rank);
        }

        // deep copy of reference ranking using alternatives deep copy ReferenceRank property
        public List<List<Alternative>> GetDeepCopyOfReferenceRanking(List<Alternative> deepCopyOfAlternatives)
        {
            var referenceRankingDeepCopy = new List<List<Alternative>>();
            var maxRank = deepCopyOfAlternatives.Select(alternative => alternative.ReferenceRank).Max();
            if (maxRank == null) return referenceRankingDeepCopy;
            for (var i = 0; i <= maxRank; i++) referenceRankingDeepCopy.Add(new List<Alternative>());
            foreach (var alternative in deepCopyOfAlternatives)
                if (alternative.ReferenceRank != null)
                    referenceRankingDeepCopy[(int)alternative.ReferenceRank].Add(alternative);
            return referenceRankingDeepCopy;
        }

        public void Reset()
        {
            foreach (var referenceRanking in RankingsCollection)
            foreach (var alternative in referenceRanking)
                alternative.ReferenceRank = null;
            RankingsCollection.Clear();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}