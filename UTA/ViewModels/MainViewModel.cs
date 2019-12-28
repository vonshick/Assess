using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;
using ImportModule;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        public readonly ObservableCollection<ChartTabViewModel> ChartTabViewModels;
        private bool _preserveKendallCoefficient = true;
        private ITab _tabToSelect;

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            Criteria = new Criteria();
            Alternatives = new Alternatives(Criteria);
            ReferenceRanking = new ReferenceRanking(0);
            Results = new Results();

            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ShowTabCommand = new RelayCommand(tabViewModel => ShowTab((ITab) tabViewModel));
            _dialogCoordinator = dialogCoordinator;

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria, Alternatives);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(Criteria, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
            ChartTabViewModels = new ObservableCollection<ChartTabViewModel>();


            // TODO: remove. for testing purposes
            Criteria.AddCriterion("Power", "", "Gain", 8);
            Criteria.AddCriterion("0-100 km/h", "", "Cost", 5);
            Criteria.CriteriaCollection[0].MinValue = Criteria.CriteriaCollection[1].MinValue = 0;
            Criteria.CriteriaCollection[0].MaxValue = Criteria.CriteriaCollection[1].MaxValue = 1;
            for (var i = 0; i < 20; i++) Alternatives.AddAlternative("Alternative X", "");

            for (var i = 0; i < Alternatives.AlternativesCollection.Count; i++)
                foreach (var criterionValue in Alternatives.AlternativesCollection[i].CriteriaValuesList)
                    criterionValue.Value = i * 0.1f;

            for (var i = 1; i < 20; i++)
            {
                ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference X"}, i);
                if (i % 2 == 0)
                    ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference XX"}, i);
                if (i % 3 == 0)
                    ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference XXX"}, i);
            }

            for (var i = 1; i <= Alternatives.AlternativesCollection.Count; i++)
                Results.FinalRanking.FinalRankingCollection.Add(new FinalRankingEntry(i, new Alternative {Name = "Final X"},
                    Alternatives.AlternativesCollection.Count - i));
        }


        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }
        public Results Results { get; set; }

        public RelayCommand ShowTabCommand { get; }
        public ObservableCollection<ITab> Tabs { get; }
        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public ReferenceRankingTabViewModel ReferenceRankingTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }


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

        public event PropertyChangedEventHandler PropertyChanged;


        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
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
                var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, "Losing current progress.",
                    "Your current partial utilities and final ranking data will be lost.\nDo you want to continue?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Yes",
                        NegativeButtonText = "Cancel",
                        AnimateShow = false, AnimateHide = false,
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

        public bool IsThereAnyApplicationProgress()
        {
            return Results.FinalRanking.FinalRankingCollection.Count != 0 || ReferenceRanking.RankingsCollection.Count != 0 ||
                   Alternatives.AlternativesCollection.Count != 0 || Criteria.CriteriaCollection.Count != 0;
        }

        public void NewSolution()
        {
            NewSolution(null, null);
        }

        public async void NewSolution(object sender, RoutedEventArgs e)
        {
            if (!IsThereAnyApplicationProgress()) return;

            var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, "Losing current progress.",
                "Your progress will be lost. Do you want to continue?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "Cancel",
                    AnimateShow = false, AnimateHide = false,
                    DefaultButtonFocus = MessageDialogResult.Affirmative
                });
            if (dialogResult != MessageDialogResult.Affirmative) return;

            Results.Reset();
            ReferenceRanking.Reset();
            Alternatives.Reset();
            Criteria.Reset();
            foreach (var chartTabViewModel in ChartTabViewModels)
                Tabs.Remove(chartTabViewModel);
            ChartTabViewModels.Clear();
        }

        [UsedImplicitly]
        public void OpenMenuItemClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "UTA Files (*.xmcda; *.xml; *.csv; *.utx)|*.xmcda;*.xml;*.csv;*.utx",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            if (openFileDialog.ShowDialog() != true) return;
            var filePath = openFileDialog.FileName;
            try
            {
                if (filePath.EndsWith(".xmcda"))
                {
                    var dataLoader = new XMCDALoader();
                    dataLoader.LoadData(filePath);
                }
                else if (filePath.EndsWith(".xml"))
                {
                    var dataLoader = new XMLLoader();
                    dataLoader.LoadData(filePath);
                }
                else if (filePath.EndsWith(".csv"))
                {
                    var dataLoader = new CSVLoader();
                    dataLoader.LoadData(filePath);
                }
                else if (filePath.EndsWith(".utx"))
                {
                    var dataLoader = new UTXLoader();
                    dataLoader.LoadData(filePath);
                }
            }
            catch (ImproperFileStructureException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}