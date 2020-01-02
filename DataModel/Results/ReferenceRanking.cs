using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Input;
using UTA.Annotations;
//using referenceRankingStructs = DataModel.Results.ReferenceRanking;

namespace DataModel.Results
{
    public class ReferenceRanking : INotifyPropertyChanged
    {
        private ObservableCollection<ObservableCollection<Alternative>> _rankingsCollection;

        public ReferenceRanking(int numOfRanksToInitialize)
        {
            RankingsCollection = new ObservableCollection<ObservableCollection<Alternative>>();
            ExpandAvailableRanksNumber(numOfRanksToInitialize);
        }

        public ObservableCollection<ObservableCollection<Alternative>> RankingsCollection
        {
            get => _rankingsCollection;
            set
            {
                if (Equals(value, _rankingsCollection)) return;
                _rankingsCollection = value;
                OnPropertyChanged(nameof(RankingsCollection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExpandAvailableRanksNumber(int size)
        {
            while (RankingsCollection.Count < size) AddRank();
        }

        public void AddRank()
        {
            var rankingCollection = new ObservableCollection<Alternative>();
            rankingCollection.CollectionChanged += CollectionChanged;
            RankingsCollection.Add(rankingCollection);
        }

        public void RemoveRank(int rank)
        {
            Console.WriteLine("Removing rank collection " + rank);
            RankingsCollection.RemoveAt(rank);
            for (int i = rank; i < RankingsCollection.Count; i++)
            {
                Console.WriteLine("Updating new collection rank " + i + " was " + (i+1));
                foreach (var alternative in RankingsCollection[i])
                {
                    alternative.ReferenceRank -= 1;
                    Console.WriteLine("updating alternative " + alternative.Name + " new rank is " + alternative.ReferenceRank);
                }
            }
        }

        public void Reset()
        {
            RankingsCollection.Clear();
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var collection = (ObservableCollection<Alternative>) sender;
                int collectionIndex = RankingsCollection.IndexOf(collection);
                if (e.NewItems.Count == collection.Count && collectionIndex == RankingsCollection.Count - 1) //add last empty rank tab
                    AddRank();
                foreach (Alternative alternative in e.NewItems)
                {
                    alternative.ReferenceRank = collectionIndex;
                    Console.WriteLine("Alternative added to rank " + alternative.ReferenceRank);
                }
            }
        }

        public void RemoveAlternativeFromRank(Alternative alternative, int rank)
        {
            RankingsCollection[rank].Remove(alternative);
        }

        public void AddAlternativeToRank(Alternative alternative, int rank)
        {
            if (RankingsCollection.Count < rank) ExpandAvailableRanksNumber(rank);
            RankingsCollection[rank - 1].Add(alternative);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}