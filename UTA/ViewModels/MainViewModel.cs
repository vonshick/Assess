using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using MahApps.Metro.Controls.Dialogs;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static Dictionary<string, string> CriterionDirectionTypes = new Dictionary<string, string>
        {
            {"g", "Gain"},
            {"c", "Cost"},
            {"o", "Ordinal"}
        };

        private readonly IDialogCoordinator _dialogCoordinator;
        public readonly ObservableCollection<ChartTabViewModel> ChartTabViewModels;

        private DataTable _alternativesTable;
        private DataTable _criteriaTable;
        private bool _preserveKendallCoefficient = true;
        private ITab _tabToSelect;


        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            Criteria = new Criteria();
            Alternatives = new Alternatives(Criteria);
            Alternatives.AlternativesCollection.CollectionChanged += AlternativesCollectionChanged;
            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ShowTabCommand = new RelayCommand(tabViewModel => ShowTab((ITab) tabViewModel));
            _dialogCoordinator = dialogCoordinator;

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria, Alternatives);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(Criteria, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
            ChartTabViewModels = new ObservableCollection<ChartTabViewModel>();


            // TODO: remove. for chart testing purposes
            Criteria.AddCriterion("Power", "", "Gain", 8);
            Criteria.AddCriterion("0-100 km/h", "", "Cost", 5);
            Criteria.CriteriaCollection[0].MinValue = Criteria.CriteriaCollection[1].MinValue = 0;
            Criteria.CriteriaCollection[0].MaxValue = Criteria.CriteriaCollection[1].MaxValue = 1;
            Alternatives.AddAlternative("Q", "");
            Alternatives.AddAlternative("W", "");
            Alternatives.AddAlternative("E", "");
            Alternatives.AddAlternative("R", "");
            Alternatives.AddAlternative("T", "");
            Alternatives.AddAlternative("Y", "");
            Alternatives.AddAlternative("U", "");
            Alternatives.AddAlternative("I", "");
            Alternatives.AddAlternative("O", "");
            Alternatives.AddAlternative("P", "");
            for (var i = 0; i < Alternatives.AlternativesCollection.Count; i++)
            {
                var alternative = Alternatives.AlternativesCollection[i];
                for (var j = 0; j < alternative.CriteriaValuesList.Count; j++)
                    alternative.CriteriaValuesList[j].Value = i * 0.1f;
            }
        }


        // TODO: remove property after using real rankings
        public Ranking Rankings { get; set; } = new Ranking();

        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public RelayCommand ShowTabCommand { get; }

        public ObservableCollection<ITab> Tabs { get; }
        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public ReferenceRankingTabViewModel ReferenceRankingTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }

        // TODO: use in proper place (to refactor)
        public bool PreserveKendallCoefficient
        {
            get => _preserveKendallCoefficient;
            set
            {
                if (_preserveKendallCoefficient == value) return;
                _preserveKendallCoefficient = value;
                OnPropertyChanged(nameof(PreserveKendallCoefficient));
            }
        }

        public ITab TabToSelect
        {
            get => _tabToSelect;
            set
            {
                _tabToSelect = value;
                OnPropertyChanged(nameof(TabToSelect));
            }
        }

        public DataTable AlternativesTable
        {
            get => _alternativesTable;
            set
            {
                if (_alternativesTable == value) return;
                _alternativesTable = value;
                OnPropertyChanged();
            }
        }

        public DataTable CriteriaTable
        {
            get => _criteriaTable;
            set
            {
                if (_criteriaTable == value) return;
                _criteriaTable = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }

        public void ShowAddCriterionDialog()
        {
            var addCriterionViewModel = new AddCriterionViewModel {MainViewModel = this, Criteria = Criteria};
            var addCriterionWindow = new AddCriterionView(addCriterionViewModel);
            addCriterionWindow.ShowDialog();
        }

        public void AddAlternative(string name, string description)
        {
            var alternative = Alternatives.AddAlternative(name, description);
            alternative.PropertyChanged += GenerateAlternativesTable;
        }

        private void TabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                ((ITab) e.NewItems[0]).CloseRequested += Tabs_Removing;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                ((ITab) e.OldItems[0]).CloseRequested -= Tabs_Removing;
        }

        private void Tabs_Removing(object sender, EventArgs e)
        {
            Tabs.Remove((ITab) sender);
        }

        [UsedImplicitly]
        public void InstancePanelAddButtonClicked(object sender, RoutedEventArgs e)
        {
            // TODO: navigate to new alternative / criterion editor on tab open
            var parentExpanderName = FindParent<Expander>(sender as UIElement).Name;
            if (parentExpanderName == "AlternativesExpander") ShowTab(AlternativesTabViewModel);
            else if (parentExpanderName == "CriteriaExpander") ShowTab(CriteriaTabViewModel);
        }

        public void ShowTab(ITab tabModel)
        {
            if (Tabs.Contains(tabModel)) TabToSelect = tabModel;
            else Tabs.Add(tabModel);
        }

        //todo not needed anymore?
        public void GenerateAlternativesTable(object sender, PropertyChangedEventArgs e)
        {
            //            GenerateAlternativesTable();
        }

        public void AddAlternativeFromDataGrid(object sender, AddingNewItemEventArgs e)
        {
            var alternative = new Alternative("initName", "initDesc", Criteria.CriteriaCollection);
            e.NewItem = alternative;
        }

        private void AlternativesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            foreach (Alternative alternative in e.NewItems)
                Alternatives.HandleNewAlternativeRanking(alternative);
        }

        public void CriterionRenamed(object sender, PropertyChangedEventArgs e)
        {
            var eExtended = (PropertyChangedExtendedEventArgs<string>) e;
            Alternatives.UpdateCriteriaValueName(eExtended.OldValue, eExtended.NewValue);
        }

        [UsedImplicitly]
        public async void SolveButtonClicked(object sender, RoutedEventArgs e)
        {
            // TODO: use flyouts maybe to inform user what to do, to begin calculations
            if (Criteria.CriteriaCollection.Count == 0)
            {
                AddTabIfNeeded(CriteriaTabViewModel);
                AddTabIfNeeded(AlternativesTabViewModel);
                AddTabIfNeeded(ReferenceRankingTabViewModel);
                ShowTab(CriteriaTabViewModel);
                return;
            }

            if (Alternatives.AlternativesCollection.Count <= 1)
            {
                AddTabIfNeeded(AlternativesTabViewModel);
                AddTabIfNeeded(ReferenceRankingTabViewModel);
                ShowTab(AlternativesTabViewModel);
                return;
            }

            var isAnyCriterionValueNull = Alternatives.AlternativesCollection.Any(alternative =>
                alternative.CriteriaValuesList.Any(criterionValue => criterionValue.Value == null));
            if (isAnyCriterionValueNull)
            {
                ShowTab(AlternativesTabViewModel);
                return;
            }

            // TODO: ShowTab(refrank) if reference ranking has more than two levels filled

            if (ChartTabViewModels.Count == 0)
            {
                ShowChartTabs();
            }
            else
            {
                var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, "Losing current progress",
                    "Your current partial utilities and final ranking data will be lost.\nDo you want to continue?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Yes", AnimateShow = false, AnimateHide = false,
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });
                if (dialogResult == MessageDialogResult.Affirmative) ShowChartTabs();
            }
        }

        private void AddTabIfNeeded(ITab tab)
        {
            if (!Tabs.Contains(tab)) Tabs.Add(tab);
        }

        private void ShowChartTabs()
        {
            foreach (var viewModel in ChartTabViewModels) Tabs.Remove(viewModel);
            ChartTabViewModels.Clear();
            foreach (var criterion in Criteria.CriteriaCollection)
            {
                var viewModel = new ChartTabViewModel(criterion, SettingsTabViewModel);
                ChartTabViewModels.Add(viewModel);
                Tabs.Add(viewModel);
            }

            if (ChartTabViewModels.Count > 0) ShowTab(ChartTabViewModels[0]);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO: remove. temporary class for designing and preview purposes
        public class Ranking
        {
            public Ranking()
            {
                for (var i = 1; i <= 20; i += 2)
                {
                    FinalRanking.Add(new FinalRankingEntry(i,
                        new Alternative($"Alternative {i}", null, CriteriaCollection),
                        (float) ((10 - i / 2) * 0.1 - 0.000001)));
                    FinalRanking.Add(new FinalRankingEntry(i + 1,
                        new Alternative($"Alternative {i + 1}", null, CriteriaCollection),
                        (float) ((10 - i / 2) * 0.1 - 0.050001)));
                }

                for (var i = 1; i <= 20; i++)
                {
                    ReferenceRanking.Add(new ReferenceRankingEntry(i,
                        new Alternative("Alternative X", null, CriteriaCollection)));
                    if (i % 2 == 0)
                        ReferenceRanking.Add(new ReferenceRankingEntry(i,
                            new Alternative("Alternative X", null, CriteriaCollection)));
                    if (i % 3 == 0)
                        ReferenceRanking.Add(new ReferenceRankingEntry(i,
                            new Alternative("Alternative X", null, CriteriaCollection)));
                }
            }

            public List<FinalRankingEntry> FinalRanking { get; set; } = new List<FinalRankingEntry>();
            public List<ReferenceRankingEntry> ReferenceRanking { get; set; } = new List<ReferenceRankingEntry>();

            public ObservableCollection<Criterion> CriteriaCollection { get; set; } =
                new ObservableCollection<Criterion>();

            public struct ReferenceRankingEntry
            {
                public int Rank { get; set; }
                public Alternative Alternative { get; set; }

                public ReferenceRankingEntry(int rank, Alternative alternative)
                {
                    Rank = rank;
                    Alternative = alternative;
                }
            }
        }

        public struct FinalRankingEntry
        {
            public int Position { get; set; }
            public Alternative Alternative { get; set; }
            public float Utility { get; set; }

            public FinalRankingEntry(int position, Alternative alternative, float utility)
            {
                Position = position;
                Alternative = alternative;
                Utility = utility;
            }
        }
    }
}