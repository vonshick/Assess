using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using UTA.Annotations;

namespace UTA.Models
{
    public class Alternatives : INotifyPropertyChanged
    {
        private ObservableCollection<Alternative> _alternativesCollection;
        private readonly Criteria _criteria;


        public Alternatives(Criteria criteria)
        {
            _criteria = criteria;
            AlternativesCollection = new ObservableCollection<Alternative>();

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(AlternativesCollection)) return;
                InitializeCriteriaMinMaxUpdaterWatcher();
            };

            _criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(_criteria.CriteriaCollection)) return;
                InitializeCriterionValueNameUpdaterWatcher();
            };

            InitializeCriteriaMinMaxUpdaterWatcher();
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

        public event PropertyChangedEventHandler PropertyChanged;


        private void InitializeCriterionValueNameUpdaterWatcher()
        {
            foreach (var criterion in _criteria.CriteriaCollection)
                InitializeCriterionNamePropertyChangedHandler(criterion);

            _criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedCriterion = (Criterion) args.NewItems[0];
                    foreach (var alternative in AlternativesCollection)
                    {
                        var newCriterionValue = new CriterionValue(addedCriterion.Name, null);
                        InitializeCriterionValueValuePropertyChangedHandler(newCriterionValue, _criteria.CriteriaCollection.Count - 1);
                        alternative.AddCriterionValue(newCriterionValue);
                    }

                    InitializeCriterionNamePropertyChangedHandler(addedCriterion);
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


        private void InitializeCriterionNamePropertyChangedHandler(Criterion criterion)
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
            foreach (var alternative in AlternativesCollection)
                for (var i = 0; i < alternative.CriteriaValuesList.Count; i++)
                    // order of criteria in CriteriaValuesList and CriteriaCollection are the same
                    InitializeCriterionValueValuePropertyChangedHandler(alternative.CriteriaValuesList[i], i);

            AlternativesCollection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedAlternative = (Alternative) e.NewItems[0];
                    for (var criterionIndex = 0; criterionIndex < addedAlternative.CriteriaValuesList.Count; criterionIndex++)
                    {
                        var criterionValue = addedAlternative.CriteriaValuesList[criterionIndex];
                        var associatedCriterion = _criteria.CriteriaCollection[criterionIndex];
                        InitializeCriterionValueValuePropertyChangedHandler(criterionValue, criterionIndex);
                        if (criterionValue.Value == null) return;
                        associatedCriterion.MinValue = Math.Min(associatedCriterion.MinValue, (double) criterionValue.Value);
                        associatedCriterion.MaxValue = Math.Max(associatedCriterion.MaxValue, (double) criterionValue.Value);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var removedAlternative = (Alternative) e.OldItems[0];
                    for (var criterionIndex = 0; criterionIndex < removedAlternative.CriteriaValuesList.Count; criterionIndex++)
                    {
                        var criterionValue = removedAlternative.CriteriaValuesList[criterionIndex];
                        if (criterionValue.Value == null) return;
                        UpdateCriterionMinMaxValueIfNeeded((double) criterionValue.Value, criterionIndex);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (var criterion in _criteria.CriteriaCollection)
                    {
                        criterion.MinValue = double.MinValue;
                        criterion.MaxValue = double.MaxValue;
                    }
                }
            };
        }

        private void InitializeCriterionValueValuePropertyChangedHandler(CriterionValue criterionValue, int criterionIndex)
        {
            criterionValue.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(criterionValue.Value)) return;
                var extendedArgs = (PropertyChangedExtendedEventArgs<double?>) e;
                var oldCriterionValueValue = extendedArgs.OldValue;
                if (oldCriterionValueValue == null) return;
                UpdateCriterionMinMaxValueIfNeeded((double) oldCriterionValueValue, criterionIndex);
            };
        }

        // sets new criterion min/max values when given changedCriterionValueOldValue was and is no longer min/max in given criterion
        private void UpdateCriterionMinMaxValueIfNeeded(double changedCriterionValueOldValue, int criterionIndex)
        {
            var criterion = _criteria.CriteriaCollection[criterionIndex];
            if (AlternativesCollection.Count == 0)
            {
                criterion.MinValue = double.MaxValue;
                criterion.MaxValue = double.MinValue;
            }
            else
            {
                if (changedCriterionValueOldValue == criterion.MinValue || criterion.MinValue == double.MaxValue)
                    criterion.MinValue = AlternativesCollection.Select(alternative =>
                        alternative.CriteriaValuesList[criterionIndex].Value is double criterionValueValue
                            ? criterionValueValue
                            : double.MaxValue).Min();
                if (changedCriterionValueOldValue == criterion.MaxValue || criterion.MaxValue == double.MinValue)
                    criterion.MaxValue = AlternativesCollection.Select(alternative =>
                        alternative.CriteriaValuesList[criterionIndex].Value is double criterionValueValue
                            ? criterionValueValue
                            : double.MinValue).Max();
            }
        }

        public List<Alternative> GetDeepCopyOfAlternatives()
        {
            var alternativesDeepCopy = new List<Alternative>();
            foreach (var alternative in AlternativesCollection)
            {
                var criteriaValuesDeepCopy = new ObservableCollection<CriterionValue>();
                foreach (var criterionValue in alternative.CriteriaValuesList)
                    criteriaValuesDeepCopy.Add(new CriterionValue(criterionValue.Name, criterionValue.Value));
                alternativesDeepCopy.Add(new Alternative
                {
                    Name = alternative.Name,
                    CriteriaValuesList = criteriaValuesDeepCopy
                });
            }

            return alternativesDeepCopy;
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