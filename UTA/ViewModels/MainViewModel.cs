using DataModel.Input;
using DataModel.PropertyChangedExtended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class MainViewModel
    {
        public static Dictionary<string, string> CriterionDirectionTypes = new Dictionary<string, string>
        {
            {"g", "Gain"},
            {"c", "Cost"},
            {"o", "Ordinal"}
        };

        private DataTable _alternativesTable;

        private DataTable _criteriaTable;
        private bool _preserveKendallCoefficient = true;

        private ITab _tabToSelect;

        public MainViewModel()
        {
            Criteria = new Criteria();
            Alternatives = new Alternatives(Criteria);
            Alternatives.AlternativesCollection.CollectionChanged += AlternativesCollectionChanged;
            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ShowTabCommand = new RelayCommand(tabModel => ShowTab((ITab)tabModel));

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria, Alternatives);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(Criteria, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
        }

        // TODO: remove property after using real rankings
        public Ranking Rankings { get; set; } = new Ranking();

        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public RelayCommand ShowTabCommand { get; }

        // TODO: use in proper place (to refactor)
        public bool PreserveKendallCoefficient
        {
            get => _preserveKendallCoefficient;
            set
            {
                _preserveKendallCoefficient = value;
                OnPropertyChanged(nameof(PreserveKendallCoefficient));
            }
        }

        public ObservableCollection<ITab> Tabs { get; }
        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public ReferenceRankingTabViewModel ReferenceRankingTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }

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
                if (_alternativesTable != value)
                {
                    _alternativesTable = value;
                    OnPropertyChanged();
                }
            }
        }

        public DataTable CriteriaTable
        {
            get => _criteriaTable;
            set
            {
                if (_criteriaTable != value)
                {
                    _criteriaTable = value;
                    OnPropertyChanged();
                }
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
            var addCriterionViewModel = new AddCriterionViewModel { MainViewModel = this, Criteria = Criteria };
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
                ((ITab)e.NewItems[0]).CloseRequested += Tabs_Removing;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                ((ITab)e.OldItems[0]).CloseRequested -= Tabs_Removing;
        }

        private void Tabs_Removing(object sender, EventArgs e)
        {
            Tabs.Remove((ITab)sender);
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //todo not needed anymore?
        public void GenerateAlternativesTable(object sender, PropertyChangedEventArgs e)
        {
            //            GenerateAlternativesTable();
        }

        public void AddAlternativeFromDataGrid(object sender, AddingNewItemEventArgs e)
        {
            Alternative alternative = new Alternative("initName", "initDesc", Criteria.CriteriaCollection);
            e.NewItem = alternative;
        }

        private void AlternativesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Alternative alternative in e.NewItems)
                {
                    Alternatives.HandleNewAlternativeRanking(alternative);
                }
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
                        (float)((10 - i / 2) * 0.1 - 0.000001)));
                    FinalRanking.Add(new FinalRankingEntry(i + 1,
                        new Alternative($"Alternative {i + 1}", null, CriteriaCollection),
                        (float)((10 - i / 2) * 0.1 - 0.050001)));
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

        public void CriterionRenamed(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedExtendedEventArgs<string> eExtended = (PropertyChangedExtendedEventArgs<string>)e;
            Alternatives.UpdateCriteriaValueName(eExtended.OldValue, eExtended.NewValue);
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