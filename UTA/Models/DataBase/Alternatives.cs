﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using DataModel.Results;
using UTA.Annotations;

namespace UTA.Models.DataBase
{
    public class Alternatives : INotifyPropertyChanged
    {
        private ObservableCollection<Alternative> _alternativesCollection;


        public Alternatives(Criteria criteria, ReferenceRanking referenceRanking)
        {
            Criteria = criteria;
            ReferenceRanking = referenceRanking;
            AlternativesCollection = new ObservableCollection<Alternative>();

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(AlternativesCollection)) return;
                InitializeWatchers();
            };

            Criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Criteria.CriteriaCollection)) return;
                InitializeCriterionValueNameUpdaterWatcher();
            };

            InitializeWatchers();
            InitializeCriterionValueNameUpdaterWatcher();
        }


        public ObservableCollection<Alternative> AlternativesCollection
        {
            get => _alternativesCollection;
            set
            {
                _alternativesCollection = value;
                OnPropertyChanged(nameof(AlternativesCollection));
            }
        }

        public ReferenceRanking ReferenceRanking { get; set; }
        public Criteria Criteria { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitializeWatchers(object sender = null, PropertyChangedEventArgs propertyChangedEventArgs = null)
        {
            InitializeReferenceRankingUsingReferenceRankAlternativeProperty();
            InitializeCriteriaMinMaxUpdaterWatcher();
        }

        private void InitializeReferenceRankingUsingReferenceRankAlternativeProperty()
        {
            var maxRank = AlternativesCollection.Select(alternative => alternative.ReferenceRank).Max();
            if (maxRank == null) return; 
            var referenceRanking = new ObservableCollection<ObservableCollection<Alternative>>();
            for (var i = 0; i <= maxRank; i++) referenceRanking.Add(new ObservableCollection<Alternative>());
            foreach (var alternative in AlternativesCollection)
                if (alternative.ReferenceRank != null)
                    referenceRanking[(int) alternative.ReferenceRank].Add(alternative);
            ReferenceRanking.RankingsCollection = referenceRanking;
        }

        private void InitializeCriterionValueNameUpdaterWatcher()
        {
            foreach (var criterion in Criteria.CriteriaCollection)
                AddCriterionNamePropertyChangedHandler(criterion);

            Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedCriterion = (Criterion) args.NewItems[0];
                    foreach (var alternative in AlternativesCollection)
                        alternative.AddCriterionValue(new CriterionValue(addedCriterion.Name, null));
                    AddCriterionNamePropertyChangedHandler(addedCriterion);
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    var removedCriterion = (Criterion) args.OldItems[0];
                    foreach (var alternative in AlternativesCollection)
                        alternative.RemoveCriterionValue(removedCriterion.Name);
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (var alternative in AlternativesCollection) alternative.CriteriaValuesList.Clear();
                }
            };
        }

        private void AddCriterionNamePropertyChangedHandler(Criterion criterion)
        {
            criterion.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(criterion.Name)) return;
                var extendedArgs = (PropertyChangedExtendedEventArgs<string>) e;
                foreach (var alternative in AlternativesCollection)
                {
                    var criterionValueToUpdate =
                        alternative.CriteriaValuesList.First(criterionValue => criterionValue.Name == extendedArgs.OldValue);
                    criterionValueToUpdate.Name = extendedArgs.NewValue;
                }
            };
        }

        private void InitializeCriteriaMinMaxUpdaterWatcher()
        {
            AlternativesCollection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedAlternative = (Alternative) e.NewItems[0];
                    foreach (var criterionValue in addedAlternative.CriteriaValuesList)
                    {
                        if (criterionValue.Value == null) return;
                        var associatedCriterion = Criteria.CriteriaCollection.First(criterion => criterion.Name == criterionValue.Name);
                        associatedCriterion.MinValue = Math.Min(associatedCriterion.MinValue, (float) criterionValue.Value);
                        associatedCriterion.MaxValue = Math.Max(associatedCriterion.MaxValue, (float) criterionValue.Value);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var removedAlternative = (Alternative) e.OldItems[0];
                    for (var i = 0; i < removedAlternative.CriteriaValuesList.Count; i++)
                    {
                        var criterionValue = removedAlternative.CriteriaValuesList[i];
                        if (criterionValue.Value == null) return;
                        var associatedCriterion = Criteria.CriteriaCollection[i];
                        if (AlternativesCollection.Count == 0)
                        {
                            associatedCriterion.MinValue = float.MaxValue;
                            associatedCriterion.MaxValue = float.MinValue;
                        }
                        else
                        {
                            if (criterionValue.Value == associatedCriterion.MinValue)
                                associatedCriterion.MinValue = AlternativesCollection.Select(alternative =>
                                    alternative.CriteriaValuesList[i].Value is float alternativeCriterionValue
                                        ? alternativeCriterionValue
                                        : float.MaxValue).Min();
                            else if (criterionValue.Value == associatedCriterion.MaxValue)
                                associatedCriterion.MaxValue = AlternativesCollection.Select(alternative =>
                                    alternative.CriteriaValuesList[i].Value is float alternativeCriterionValue
                                        ? alternativeCriterionValue
                                        : float.MinValue).Max();
                        }
                    }
                }
            };
        }

        public void Reset()
        {
            AlternativesCollection.Clear();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}