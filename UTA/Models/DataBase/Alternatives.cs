using DataModel.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public ObservableCollection<Alternative> AlternativesCollection { get; set; }
        public ObservableCollection<Alternative> AlternativesNotRankedCollection { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }
        public Criteria Criteria { get; set; }
        public Alternative Placeholder { get; set; } = null;

        public Alternatives(Criteria criteria)
        {
            Criteria = criteria;
            AlternativesCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection.CollectionChanged += CollectionChanged;
            ReferenceRanking = new ReferenceRanking(1);
        }

        public Alternative AddAlternative(string name, string description)
        {
            Alternative alternative = new Alternative(name, description, Criteria.CriteriaCollection);
            AlternativesCollection.Add(alternative);
            return alternative;
        }
        public void AddAlternative(Alternative alternative)
        {
            AlternativesCollection.Add(alternative);
        }

        public void AddNewCriterionToAlternatives(string name, float? value)
        {
            foreach (var alternative in AlternativesCollection)
            {
                CriterionValue criterionValue = new CriterionValue(name, value);
                alternative.AddCriterionValue(criterionValue);
            }
        }

        public void HandleNewAlternativeRanking(Alternative alternative)
        {
            if (alternative.ReferenceRank != null)
            {
                ReferenceRanking.AddAlternativeToRank(alternative, alternative.ReferenceRank.Value);
            }
            else
            {
                AlternativesNotRankedCollection.Add(alternative);
            }
        }

        public void RemoveAlternative(Alternative alternative)
        {
            int? rank = alternative.ReferenceRank;
            if (rank != null)
            {
                Console.WriteLine("Removing ranked alternative " + alternative.Name);
                ReferenceRanking.RemoveAlternativeFromRank(alternative, rank.Value);
            }
            else
            {
                Console.WriteLine("Removing  NOT ranked alternative " + alternative.Name);
                AlternativesNotRankedCollection.Remove(alternative);
            }
            AlternativesCollection.Remove(alternative);
        }

        public void RemoveAlternativeFromRank(Alternative alternative)
        {
            Console.WriteLine("Removing alt " + alternative.Name + " from rank " + alternative.ReferenceRank + " and adding to NotRanked");
            ReferenceRanking.RemoveAlternativeFromRank(alternative, alternative.ReferenceRank.Value);
            AlternativesNotRankedCollection.Add(alternative);
        }

        public void RemoveRank(int rank)
        {
            List<Alternative> tempCollection = ReferenceRanking.RankingsCollection[rank].ToList();
            foreach (var alternative in tempCollection)
            {
                RemoveAlternativeFromRank(alternative);
            }
            ReferenceRanking.RemoveRank(rank);
        }

        public void UpdateCriteriaValueName(string oldName, string newName)
        {
            foreach (Alternative alternative in AlternativesCollection)
            {
                alternative.UpdateCriterionValueName(oldName, newName);
            }
        }

        public void AddPlaceholder()
        {
            Placeholder = AddAlternative("name", "description");
        }

        public void RemovePlaceholder()
        {
            AlternativesCollection.Remove(Placeholder);
        }
        public void SaveCurrentPlaceholder()
        {
            AddAlternative(Placeholder);
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("alt coll changed");
            if (e.NewItems != null)
            {
                foreach (Alternative alternative in e.NewItems)
                {
                    alternative.ReferenceRank = null;
                }
            }
        }
    }
}
