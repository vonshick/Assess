using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public ObservableCollection<Alternative> AlternativesCollection { get; set; }
        public ObservableCollection<Alternative> AlternativesNotRankedCollection { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }

        public Criteria Criteria { get; set; }

        public Alternatives(Criteria criteria)
        {
            Criteria = criteria;
            AlternativesCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection.CollectionChanged += CollectionChanged;
            ReferenceRanking = new ReferenceRanking(4);
        }

        public Alternative AddAlternative(string name, string description)
        {
            Alternative alternative = new Alternative(name, description, Criteria.CriteriaCollection);
            AlternativesCollection.Add(alternative);
            return alternative;
        }

        public void AddNewCriterionToAlternatives(string name, float value)
        {
            foreach (var alternative in AlternativesCollection)
            {
                CriterionValue criterionValue = new CriterionValue(name, value);
                alternative.AddCriterionValue(criterionValue);
            }
        }

        public void HandleNewAlternativeRanking(Alternative alternative)
        {
            if (alternative.ReferenceRank != -1)
            {
                ReferenceRanking.AddAlternativeToRank(alternative);
            }
            else
            {
                AlternativesNotRankedCollection.Add(alternative);
            }
        }

        public void RemoveAlternative(Alternative alternative)
        {
            int rank = alternative.ReferenceRank;
            if (rank != -1)
            {
                Console.WriteLine("Removing ranked alternative " + alternative.Name);
                ReferenceRanking.RemoveAlternativeFromRank(alternative);
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
            ReferenceRanking.RemoveAlternativeFromRank(alternative);
            AlternativesNotRankedCollection.Add(alternative);
        }

        public void UpdateCriteriaValueName(string oldName, string newName)
        {
            foreach (Alternative alternative in AlternativesCollection)
            {
                alternative.UpdateCriterionValueName(oldName, newName);
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Alternative alternative in e.NewItems)
                {
                    alternative.ReferenceRank = -1;
                }
            }
        }
    }
}
