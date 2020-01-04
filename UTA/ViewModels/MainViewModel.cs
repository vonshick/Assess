using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CalculationsEngine;
using DataModel.Input;
using DataModel.Results;
using ExportModule;
using ImportModule;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        public readonly ObservableCollection<ChartTabViewModel> ChartTabViewModels;
        private bool _preserveKendallCoefficient = true;
        private SaveData _saveData;
        private ITab _tabToSelect;

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            Criteria = new Criteria();
            ReferenceRanking = new ReferenceRanking(0);
            Alternatives = new Alternatives(Criteria, ReferenceRanking);
            Results = new Results();

            _dialogCoordinator = dialogCoordinator;
            _saveData = new SaveData(null, null);

            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ShowTabCommand = new RelayCommand(tabViewModel => ShowTab((ITab) tabViewModel));

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria, Alternatives);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(Criteria, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
            ChartTabViewModels = new ObservableCollection<ChartTabViewModel>();

            Criteria.CriteriaCollection.CollectionChanged += InstancePropertyChanged;
            Alternatives.AlternativesCollection.CollectionChanged += InstancePropertyChanged;
            ReferenceRanking.RankingsCollection.CollectionChanged += InstancePropertyChanged;
            Results.FinalRanking.FinalRankingCollection.CollectionChanged += InstancePropertyChanged;
            Criteria.PropertyChanged += InstancePropertyChanged;
            Alternatives.PropertyChanged += InstancePropertyChanged;
            ReferenceRanking.PropertyChanged += InstancePropertyChanged;
            Results.FinalRanking.PropertyChanged += InstancePropertyChanged;
            Results.PropertyChanged += InstancePropertyChanged;

            // TODO: remove. for testing purposes
            //Criteria.AddCriterion("Power", "", "Gain", 8);
            //Criteria.AddCriterion("0-100 km/h", "", "Cost", 5);
            //Criteria.CriteriaCollection[0].MinValue = Criteria.CriteriaCollection[1].MinValue = 0;
            //Criteria.CriteriaCollection[0].MaxValue = Criteria.CriteriaCollection[1].MaxValue = 1;
            //for (var i = 0; i < 20; i++) Alternatives.AddAlternative("Alternative X", "");

            //for (var i = 0; i < Alternatives.AlternativesCollection.Count; i++)
            //    foreach (var criterionValue in Alternatives.AlternativesCollection[i].CriteriaValuesList)
            //        criterionValue.Value = i * 0.1f;

            //for (var i = 1; i < 20; i++)
            //{
            //    ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference X"}, i);
            //    if (i % 2 == 0)
            //        ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference XX"}, i);
            //    if (i % 3 == 0)
            //        ReferenceRanking.AddAlternativeToRank(new Alternative {Name = "Reference XXX"}, i);
            //}

            //for (var i = 1; i <= Alternatives.AlternativesCollection.Count; i++)
            //    Results.FinalRanking.FinalRankingCollection.Add(new FinalRankingEntry(i, new Alternative {Name = "Final X"},
            //        Alternatives.AlternativesCollection.Count - i));
        }


        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }
        public Results Results { get; set; }

        public ObservableCollection<ITab> Tabs { get; }
        public RelayCommand ShowTabCommand { get; }

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

        public bool IsThereAnyApplicationProgress =>
            Results.FinalRanking.FinalRankingCollection.Count != 0
            || Results.PartialUtilityFunctions.Count != 0
            || ReferenceRanking.RankingsCollection.Count != 0
            || Alternatives.AlternativesCollection.Count != 0
            || Criteria.CriteriaCollection.Count != 0;

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

            if (!(ReferenceRanking.RankingsCollection.Count >= 2
                  && ReferenceRanking.RankingsCollection[0].Count != 0
                  && ReferenceRanking.RankingsCollection[1].Count != 0))
            {
                ShowTab(ReferenceRankingTabViewModel);
                return;
            }

            if (ChartTabViewModels.Count == 0)
            {
                ShowChartTabs();
            }
            else
            {
                var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, "Losing current progress.",
                    "Your current partial utilities and final ranking data will be lost.\n" +
                    "If you accidentally closed some partial utility plots, you can show them again by using Show menu option.\n" +
                    "Do you want to continue?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Yes",
                        NegativeButtonText = "Cancel",
                        DefaultButtonFocus = MessageDialogResult.Affirmative,
                        AnimateShow = false,
                        AnimateHide = false
                    });
                if (dialogResult == MessageDialogResult.Affirmative) ShowChartTabs();
            }
        }

        private void AddTabIfNeeded(ITab tab)
        {
            if (!Tabs.Contains(tab)) Tabs.Add(tab);
        }

        private async void ShowChartTabs()
        {
            foreach (var viewModel in ChartTabViewModels) Tabs.Remove(viewModel);
            ChartTabViewModels.Clear();

            var invalidCriteriaValuesNames = new List<string>();
            foreach (var criterion in Criteria.CriteriaCollection)
                if (Math.Abs(criterion.MaxValue - criterion.MinValue) < 0.000000001)
                    invalidCriteriaValuesNames.Add(criterion.Name);

            if (invalidCriteriaValuesNames.Count != 0)
            {
                ShowTab(AlternativesTabViewModel);

                var warningMessage = "Alternatives values on the following criteria have too high precision or are the same:\n";
                foreach (var criterionName in invalidCriteriaValuesNames) warningMessage += $"{criterionName},\n";
                warningMessage +=
                    "Please provide lower precision values or at least two unique values on a whole set of alternatives values.";
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Invalid alternatives values.",
                    warningMessage,
                    MessageDialogStyle.Affirmative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        DefaultButtonFocus = MessageDialogResult.Affirmative,
                        AnimateShow = false,
                        AnimateHide = false
                    });
                return;
            }

            var solver = new Solver(ReferenceRanking, new List<Criterion>(Criteria.CriteriaCollection),
                new List<Alternative>(Alternatives.AlternativesCollection), Results);
            solver.Calculate();
            foreach (var partialUtility in Results.PartialUtilityFunctions)
            {
                var viewModel = new ChartTabViewModel(solver, partialUtility, SettingsTabViewModel, RefreshCharts);
                ChartTabViewModels.Add(viewModel);
                Tabs.Add(viewModel);
            }

            if (ChartTabViewModels.Count > 0) ShowTab(ChartTabViewModels[0]);
        }

        private void RefreshCharts()
        {
            // TODO: optional. consider using faster method than GenerateChartData
            foreach (var chartTabViewModel in ChartTabViewModels) chartTabViewModel.GenerateChartData();
        }

        // xaml enforces void return type
        [UsedImplicitly]
        public async void NewSolution(object sender, RoutedEventArgs e)
        {
            await NewSolution();
        }

        public async Task<bool> NewSolution()
        {
            if (!IsThereAnyApplicationProgress) return true;

            var saveDialogResult = await _dialogCoordinator.ShowMessageAsync(this,
                "Losing current progress.",
                "Your progress will be lost. Would you like to save it before continuing?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    FirstAuxiliaryButtonText = "Cancel",
                    DialogResultOnCancel = MessageDialogResult.FirstAuxiliary,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (saveDialogResult == MessageDialogResult.FirstAuxiliary) return false;
            if (saveDialogResult == MessageDialogResult.Affirmative)
                await SaveTypeChooserDialog();

            Results.Reset();
            ReferenceRanking.Reset();
            Alternatives.Reset();
            Criteria.Reset();
            foreach (var chartTabViewModel in ChartTabViewModels)
                Tabs.Remove(chartTabViewModel);
            ChartTabViewModels.Clear();
            _saveData.IsSavingWithResults = null;
            _saveData.FilePath = null;
            return true;
        }

        private async Task SaveTypeChooserDialog()
        {
            var saveFileWithResultsDialog = await _dialogCoordinator.ShowMessageAsync(this,
                "Choose save type.",
                "Would you like to save your progress with or without results?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Save With Results",
                    NegativeButtonText = "Save Without Results",
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (saveFileWithResultsDialog == MessageDialogResult.Affirmative) SaveWithResultsAsMenuItemClicked();
            else SaveAsMenuItemClicked();
        }

        [UsedImplicitly]
        public async void OpenFileMenuItemClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "UTA Input Files (*.xml; *.csv; *.utx)|*.xml;*.csv;*.utx",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            if (openFileDialog.ShowDialog() != true) return;

            if (!await NewSolution()) return;

            var filePath = openFileDialog.FileName;
            try
            {
                DataLoader dataLoader = null;
                if (filePath.EndsWith(".xml")) dataLoader = new XMLLoader();
                else if (filePath.EndsWith(".csv")) dataLoader = new CSVLoader();
                else if (filePath.EndsWith(".utx")) dataLoader = new UTXLoader();

                if (dataLoader == null) return;

                dataLoader.LoadData(filePath);
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
            }
            catch (Exception exception)
            {
                await ShowLoadErrorDialog(exception);
            }
        }

        [UsedImplicitly]
        public async void OpenXMCDAMenuItemClicked(object sender, RoutedEventArgs e)
        {
            var openDirectoryDialog = new VistaFolderBrowserDialog {ShowNewFolderButton = true};
            if (openDirectoryDialog.ShowDialog() != true) return;

            if (!await NewSolution()) return;

            var filePath = openDirectoryDialog.SelectedPath;
            try
            {
                var dataLoader = new XMCDALoader();
                dataLoader.LoadData(filePath);
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
                // TODO: get reference and final ranking, and partial utilities (vonshick needs to update his modules).
            }
            catch (Exception exception)
            {
                await ShowLoadErrorDialog(exception);
            }
        }

        private async Task ShowLoadErrorDialog(Exception exception)
        {
            await _dialogCoordinator.ShowMessageAsync(this,
                "Loading error.",
                exception is ImproperFileStructureException ifsException && ifsException.Message != null
                    ? $"Can't read this file. An error was encountered with a following message:\n\"{exception.Message}\""
                    : "Can't read this file.",
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    AnimateShow = false,
                    AnimateHide = false,
                    DefaultButtonFocus = MessageDialogResult.Affirmative
                });
        }

        public async void SaveMenuItemClicked(object sender = null, RoutedEventArgs e = null)
        {
            if (_saveData.IsSavingWithResults == null || _saveData.FilePath == null)
            {
                await SaveTypeChooserDialog();
                return;
            }

            var dataSaver = new XMCDAExporter(_saveData.FilePath, new List<Criterion>(Criteria.CriteriaCollection),
                new List<Alternative>(Alternatives.AlternativesCollection), Results);
            if (_saveData.IsSavingWithResults == true) dataSaver.saveSession();
            else dataSaver.saveInput();
        }

        public void SaveAsMenuItemClicked(object sender = null, RoutedEventArgs e = null)
        {
            var saveXMCDADialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "Select XMCDA Output Directory"
            };
            if (saveXMCDADialog.ShowDialog() != true) return;

            var directoryPath = saveXMCDADialog.SelectedPath;
            var dataSaver = new XMCDAExporter(directoryPath, new List<Criterion>(Criteria.CriteriaCollection),
                new List<Alternative>(Alternatives.AlternativesCollection), Results);

            // TODO: add force saving (in case of overwrite)
            dataSaver.saveInput();
            _saveData.IsSavingWithResults = false;
            _saveData.FilePath = directoryPath;
        }

        public void SaveWithResultsAsMenuItemClicked(object sender = null, RoutedEventArgs e = null)
        {
            var saveXMCDADialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "Select XMCDA Output Directory"
            };
            if (saveXMCDADialog.ShowDialog() != true) return;

            var directoryPath = saveXMCDADialog.SelectedPath;
            var dataSaver = new XMCDAExporter(directoryPath, new List<Criterion>(Criteria.CriteriaCollection),
                new List<Alternative>(Alternatives.AlternativesCollection), Results);

            if (Results.FinalRanking.FinalRankingCollection.Count != 0 && Results.PartialUtilityFunctions.Count != 0)
                dataSaver.saveInput();
            else dataSaver.saveSession();
            _saveData.IsSavingWithResults = true;
            _saveData.FilePath = directoryPath;
        }

        private void InstancePropertyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsThereAnyApplicationProgress));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private struct SaveData
        {
            public bool? IsSavingWithResults;
            [CanBeNull] public string FilePath;

            public SaveData(bool? isSavingWithResults, [CanBeNull] string filePath)
            {
                IsSavingWithResults = isSavingWithResults;
                FilePath = filePath;
            }
        }
    }
}