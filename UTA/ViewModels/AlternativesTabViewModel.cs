using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using DataModel.Annotations;
using DataModel.Input;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
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
                               NewAlternative.CriteriaValuesList.TrueForAll(criterionValue => criterionValue.Value != null));
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
                if (_newAlternative != null && value.Name == _newAlternative.Name) return;
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
            Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                    NewAlternative.AddCriterionValue(new CriterionValue(((Criterion) args.NewItems[0]).Name, null));
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                    NewAlternative.RemoveCriterionValue(((Criterion) args.OldItems[0]).Name);
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                    InitializeNewAlternative();
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