using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using DataModel.Annotations;
using DataModel.Input;
using UTA.Helpers;
using UTA.Models;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class CriteriaTabViewModel : Tab, INotifyPropertyChanged
    {
        private int _criterionIndexToShow = -1;
        private bool _nameTextBoxFocusTrigger;
        private Criterion _newCriterion;


        public CriteriaTabViewModel(Criteria criteria)
        {
            Name = "Criteria";
            Criteria = criteria;

            Criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Criteria.CriteriaCollection)) return;
                InitializeCriterionIndexToShowWatcher();
            };
            InitializeCriterionIndexToShowWatcher();

            InitializeNewCriterion();

            RemoveCriterionCommand = new RelayCommand(criterion => Criteria.CriteriaCollection.Remove((Criterion) criterion));
            AddCriterionCommand = new RelayCommand(_ =>
            {
                NewCriterion.Name = NewCriterion.Name.Trim(' ');
                NewCriterion.Description = NewCriterion.Description.Trim(' ');
                Criteria.CriteriaCollection.Add(NewCriterion);
                InitializeNewCriterion();
            }, bindingGroup => !((BindingGroup) bindingGroup).HasValidationError && NewCriterion.Name != "");
        }


        public Criteria Criteria { get; }
        public RelayCommand RemoveCriterionCommand { get; }
        public RelayCommand AddCriterionCommand { get; }

        public Criterion NewCriterion
        {
            get => _newCriterion;
            set
            {
                _newCriterion = value;
                OnPropertyChanged(nameof(NewCriterion));
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

        public int CriterionIndexToShow
        {
            get => _criterionIndexToShow;
            set
            {
                _criterionIndexToShow = value;
                OnPropertyChanged(nameof(CriterionIndexToShow));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;


        private void InitializeCriterionIndexToShowWatcher()
        {
            if (Criteria.CriteriaCollection.Count == 0) CriterionIndexToShow = -1;

            Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (Criteria.CriteriaCollection.Count == 0) CriterionIndexToShow = -1;
            };
        }

        private void InitializeNewCriterion()
        {
            NewCriterion = new Criterion("", "", Criterion.CriterionDirectionTypesList[0]);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}