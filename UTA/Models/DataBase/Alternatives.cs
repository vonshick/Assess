using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Input;
using DataModel.Results;
using UTA.Annotations;

namespace UTA.Models.DataBase
{
    public class Alternatives : INotifyPropertyChanged
    {
        private ObservableCollection<Alternative> _alternativesCollection;
        private ObservableCollection<Alternative> _alternativesNotRankedCollection;

        public Alternatives(Criteria criteria, ReferenceRanking referenceRanking)
        {
            Criteria = criteria;
            ReferenceRanking = referenceRanking;
            AlternativesCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection = new ObservableCollection<Alternative>();
            AlternativesNotRankedCollection.CollectionChanged += CollectionChanged;
        }


        public ObservableCollection<Alternative> AlternativesCollection
        {
            get => _alternativesCollection;
            set
            {
                if (Equals(value, _alternativesCollection)) return;
                _alternativesCollection = value;
                OnPropertyChanged(nameof(AlternativesCollection));
            }
        }

        public ObservableCollection<Alternative> AlternativesNotRankedCollection
        {
            get => _alternativesNotRankedCollection;
            set
            {
                if (Equals(value, _alternativesNotRankedCollection)) return;
                _alternativesNotRankedCollection = value;
                OnPropertyChanged(nameof(AlternativesNotRankedCollection));
            }
        }

        public ReferenceRanking ReferenceRanking { get; set; }
        public Criteria Criteria { get; set; }
        public Alternative Placeholder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Alternative AddAlternative(string name, string description)
        {
            var alternative = new Alternative(name, description, Criteria.CriteriaCollection);
            AlternativesCollection.Add(alternative);
            return alternative;
        }

        public void AddNewCriterionToAlternatives(string name, float? value)
        {
            Console.WriteLine("Adding to alternatives value (" + name + "," + value + ")");
            foreach (var alternative in AlternativesCollection)
            {
                var criterionValue = new CriterionValue(name, value);
                alternative.AddCriterionValue(criterionValue);
            }
        }

        public void HandleNewAlternativeRanking(Alternative alternative)
        {
            if (alternative.ReferenceRank != null)
                ReferenceRanking.AddAlternativeToRank(alternative, alternative.ReferenceRank.Value);
            else
                AlternativesNotRankedCollection.Add(alternative);
        }

        public void RemoveAlternative(Alternative alternative)
        {
            var rank = alternative.ReferenceRank;
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
            var rankAlternativesList = ReferenceRanking.RankingsCollection[rank].ToList();
            foreach (var alternative in rankAlternativesList) RemoveAlternativeFromRank(alternative);
            ReferenceRanking.RemoveRank(rank);
        }

        public void UpdateCriteriaValueName(string oldName, string newName)
        {
            Console.WriteLine("Updating alternatives: set crit name from " + oldName + " to " + newName);
            foreach (var alternative in AlternativesCollection) alternative.UpdateCriterionValueName(oldName, newName);
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
            AlternativesCollection.Add(Placeholder);
        }

        public void Reset()
        {
            AlternativesCollection.Clear();
            AlternativesNotRankedCollection.Clear();
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("alt coll changed");
            if (e.NewItems != null)
                foreach (Alternative alternative in e.NewItems)
                    alternative.ReferenceRank = null;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}