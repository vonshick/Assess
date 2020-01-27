using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Assess.Helpers;
using Assess.Models;
using Assess.Models.Tab;
using Assess.Properties;
using DataModel.Input;
using DataModel.PropertyChangedExtended;

namespace Assess.ViewModels
{
    public class AlternativesTabViewModel : Tab, INotifyPropertyChanged
    {
        private int _alternativeIndexToShow = -1;
        private bool _nameTextBoxFocusTrigger;
        private Alternative _newAlternative;


        public AlternativesTabViewModel(Criteria criteria, Alternatives alternatives)
        {
            Name = "Alternatives";
            Criteria = criteria;
            Alternatives = alternatives;

            Criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Criteria.CriteriaCollection)) return;
                InitializeNewAlternative();
                InitializeNewAlternativeCriterionValuesUpdaterWatcher();
            };

            Alternatives.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Alternatives.AlternativesCollection)) return;
                InitializeAlternativeIndexToShowWatcher();
            };

            InitializeNewAlternative();
            InitializeNewAlternativeCriterionValuesUpdaterWatcher();
            InitializeAlternativeIndexToShowWatcher();

            RemoveAlternativeCommand =
                new RelayCommand(alternative => Alternatives.AlternativesCollection.Remove((Alternative) alternative));
            AddAlternativeCommand = new RelayCommand(_ =>
            {
                NewAlternative.Name = NewAlternative.Name.Trim(' ');
                Alternatives.AlternativesCollection.Add(NewAlternative);
                InitializeNewAlternative();
            }, bindingGroup => !((BindingGroup) bindingGroup).HasValidationError && NewAlternative.Name != "" &&
                               NewAlternative.CriteriaValuesList.All(criterionValue => criterionValue.Value != null));
        }


        public Criteria Criteria { get; }
        public Alternatives Alternatives { get; }
        public RelayCommand RemoveAlternativeCommand { get; }
        public RelayCommand AddAlternativeCommand { get; }

        public Alternative NewAlternative
        {
            get => _newAlternative;
            set
            {
                _newAlternative = value;
                OnPropertyChanged(nameof(NewAlternative));
            }
        }

        public bool NameTextBoxFocusTrigger
        {
            get => _nameTextBoxFocusTrigger;
            set
            {
                if (value == _nameTextBoxFocusTrigger) return;
                _nameTextBoxFocusTrigger = value;
                OnPropertyChanged(nameof(NameTextBoxFocusTrigger));
            }
        }

        public int AlternativeIndexToShow
        {
            get => _alternativeIndexToShow;
            set
            {
                _alternativeIndexToShow = value;
                OnPropertyChanged(nameof(AlternativeIndexToShow));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void InitializeAlternativeIndexToShowWatcher()
        {
            if (Alternatives.AlternativesCollection.Count == 0) AlternativeIndexToShow = -1;

            Alternatives.AlternativesCollection.CollectionChanged += (sender, args) =>
            {
                if (Alternatives.AlternativesCollection.Count == 0) AlternativeIndexToShow = -1;
            };
        }

        public void InitializeNewAlternativeCriterionValuesUpdaterWatcher()
        {
            foreach (var criterion in Criteria.CriteriaCollection)
                AddCriterionNamePropertyChangedHandler(criterion);

            Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedCriterion = (Criterion) args.NewItems[0];
                    NewAlternative.AddCriterionValue(new CriterionValue(addedCriterion.Name, null));
                    AddCriterionNamePropertyChangedHandler(addedCriterion);
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    NewAlternative.RemoveCriterionValue(((Criterion) args.OldItems[0]).Name);
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    InitializeNewAlternative();
                }
            };
        }

        private void AddCriterionNamePropertyChangedHandler(Criterion criterion)
        {
            criterion.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(criterion.Name)) return;
                var extendedArgs = (PropertyChangedExtendedEventArgs<string>) e;
                var criterionValueToUpdate =
                    NewAlternative.CriteriaValuesList.First(criterionValue => criterionValue.Name == extendedArgs.OldValue);
                criterionValueToUpdate.Name = extendedArgs.NewValue;
            };
        }

        private void InitializeNewAlternative()
        {
            NewAlternative = new Alternative("", Criteria.CriteriaCollection);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}