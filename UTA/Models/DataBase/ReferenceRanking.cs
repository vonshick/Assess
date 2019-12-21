using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class ReferenceRanking
    {
        public ReferenceRanking(int numOfRanksToInitialize)
        {
            RankingsCollection = new ObservableCollection<ObservableCollection<Alternative>>();
            ExpandAvailableRanksNumber(numOfRanksToInitialize);
        }
        private void ExpandAvailableRanksNumber(int size)
        {
            while (RankingsCollection.Count < size)
            {
                AddRank();
            }
        }

        public void AddRank()
        {
            ObservableCollection<Alternative> rankingCollection = new ObservableCollection<Alternative>();
            rankingCollection.CollectionChanged += CollectionChanged;
            RankingsCollection.Add(rankingCollection);
        }

        public void RemoveRank(int rank)
        {
            RankingsCollection.RemoveAt(rank);
        }

        public ObservableCollection<ObservableCollection<Alternative>> RankingsCollection { get; set; }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Alternative alternative in e.NewItems)
                {
                    alternative.ReferenceRank = RankingsCollection.IndexOf((ObservableCollection<Alternative>)sender) + 1;
                    Console.WriteLine("Alternative added to new rank " + alternative.ReferenceRank);
                }
            }
        }

        public void RemoveAlternativeFromRank(Alternative alternative, int rank)
        {
            RankingsCollection[rank - 1].Remove(alternative);
        }

        public void AddAlternativeToRank(Alternative alternative, int rank)
        {
            if (RankingsCollection.Count < rank) ExpandAvailableRanksNumber(rank);
            RankingsCollection[rank - 1].Add(alternative);
        }
    }
}
