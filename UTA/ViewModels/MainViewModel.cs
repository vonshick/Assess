using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CalculationsEngine;
using DataModel.Input;
using DataModel.Results;
using ExportModule;
using ImportModule;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;
using UTA.Annotations;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private bool _preserveKendallCoefficient = true;
        private SaveData _saveData;
        private ITab _tabToSelect;

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            Criteria = new Criteria();
            ReferenceRanking = new ReferenceRanking();
            Alternatives = new Alternatives(Criteria, ReferenceRanking);
            Results = new Results();

            _dialogCoordinator = dialogCoordinator;
            _saveData = new SaveData(null, null);

            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ChartTabViewModels = new ObservableCollection<ChartTabViewModel>();
            ShowTabCommand = new RelayCommand(tabViewModel => ShowTab((ITab) tabViewModel));

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(ReferenceRanking, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();

            Criteria.CriteriaCollection.CollectionChanged += InstancePropertyChanged;
            Alternatives.AlternativesCollection.CollectionChanged += InstancePropertyChanged;
            ReferenceRanking.RankingsCollection.CollectionChanged += InstancePropertyChanged;
            Results.FinalRanking.FinalRankingCollection.CollectionChanged += InstancePropertyChanged;

            Criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Criteria.CriteriaCollection)) return;
                InstancePropertyChanged();
                Criteria.CriteriaCollection.CollectionChanged += InstancePropertyChanged;
            };
            Alternatives.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Alternatives.AlternativesCollection)) return;
                InstancePropertyChanged();
                Alternatives.AlternativesCollection.CollectionChanged += InstancePropertyChanged;
            };
            ReferenceRanking.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(ReferenceRanking.RankingsCollection)) return;
                InstancePropertyChanged();
                ReferenceRanking.RankingsCollection.CollectionChanged += InstancePropertyChanged;
            };
            Results.FinalRanking.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Results.FinalRanking.FinalRankingCollection)) return;
                InstancePropertyChanged();
                Results.FinalRanking.FinalRankingCollection.CollectionChanged += InstancePropertyChanged;
            };


            // TODO: remove. for testing purposes
            // WARNING: using these crashes application at some point
            //Criteria.CriteriaCollection.Add(new Criterion("A", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection.Add(new Criterion("B", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection.Add(new Criterion("C", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection.Add(new Criterion("D", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection.Add(new Criterion("E", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection.Add(new Criterion("F", "ABC", "Gain", 8));
            //Criteria.CriteriaCollection[0].MinValue = Criteria.CriteriaCollection[1].MinValue = 0;
            //Criteria.CriteriaCollection[0].MaxValue = Criteria.CriteriaCollection[1].MaxValue = 1;
            //for (var i = 0; i < 20; i++)
            //    Alternatives.AlternativesCollection.Add(new Alternative($"Alternative X{i}", new ObservableCollection<Criterion>()));

            //for (var i = 0; i < Alternatives.AlternativesCollection.Count; i++)
            //    foreach (var criterionValue in Alternatives.AlternativesCollection[i].CriteriaValuesList)
            //        criterionValue.Value = i * 0.1f;

            //for (var i = 0; i < 20; i++)
            //{
            //    ReferenceRanking.AddAlternativeToRank(new Alternative { Name = "Reference X" }, i);
            //    if (i % 2 == 0)
            //        ReferenceRanking.AddAlternativeToRank(new Alternative { Name = "Reference XX" }, i);
            //    if (i % 3 == 0)
            //        ReferenceRanking.AddAlternativeToRank(new Alternative { Name = "Reference XXX" }, i);
            //}

            //for (var i = 0; i < Alternatives.AlternativesCollection.Count; i++)
            //    Results.FinalRanking.FinalRankingCollection.Add(new FinalRankingEntry(i, Alternatives.AlternativesCollection[i],
            //        0.191919f));

            // TODO vonshick REMOVE IT AFTER TESTING
            // string dataDirectoryPath = "D:\\Data";
            // DataLoader dataLoader = SampleImport.ProcessSampleData(dataDirectoryPath); // csv
            // SampleExport.exportXMCDA(dataDirectoryPath, dataLoader.CriterionList, dataLoader.AlternativeList);
        }


        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }
        public Results Results { get; set; }

        public ObservableCollection<ITab> Tabs { get; }
        public ObservableCollection<ChartTabViewModel> ChartTabViewModels { get; }
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

        public void ShowTab(ITab tabModel)
        {
            if (Tabs.Contains(tabModel)) TabToSelect = tabModel;
            else Tabs.Add(tabModel);
        }

        [UsedImplicitly]
        public async void CalculateButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Criteria.CriteriaCollection.Count == 0)
            {
                ShowCalculateErrorDialog("It's required to provide at least 1 criterion to begin UTA calculations.");
                AddTabIfNeeded(CriteriaTabViewModel);
                AddTabIfNeeded(AlternativesTabViewModel);
                AddTabIfNeeded(ReferenceRankingTabViewModel);
                ShowTab(CriteriaTabViewModel);
                return;
            }

            if (Alternatives.AlternativesCollection.Count <= 1)
            {
                ShowCalculateErrorDialog("It's required to provide at least 2 alternatives to begin UTA calculations.");
                AddTabIfNeeded(AlternativesTabViewModel);
                AddTabIfNeeded(ReferenceRankingTabViewModel);
                ShowTab(AlternativesTabViewModel);
                return;
            }

            var isAnyCriterionValueNull = Alternatives.AlternativesCollection.Any(alternative =>
                alternative.CriteriaValuesList.Any(criterionValue => criterionValue.Value == null));
            if (isAnyCriterionValueNull)
            {
                ShowCalculateErrorDialog(
                    "It's required to provide data to every criterion value to all alternatives to begin UTA calculations.");
                ShowTab(AlternativesTabViewModel);
                return;
            }

            if (!(ReferenceRanking.RankingsCollection.Count >= 2
                  && ReferenceRanking.RankingsCollection.All(rank => rank.Count != 0)))
            {
                ShowCalculateErrorDialog(
                    "It's required to provide at least 2 ranks in Reference Ranking filled with at least 1 alternative\nto begin UTA calculations.");
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

        private async void ShowCalculateErrorDialog(string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this,
                "Invalid instance data.",
                message,
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false,
                    AnimateHide = false
                });
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
                ShowLoadErrorDialog(exception);
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
                // TODO: check if everything works (vonshick)
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                // works assuming that CriteriaValuesList and ReferenceRank property are initialized properly
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
                Results.KendallCoefficient = dataLoader.Results.KendallCoefficient;
                Results.PartialUtilityFunctions = dataLoader.Results.PartialUtilityFunctions;
                Results.FinalRanking.FinalRankingCollection = dataLoader.Results.FinalRanking.FinalRankingCollection;
            }
            catch (Exception exception)
            {
                ShowLoadErrorDialog(exception);
            }
        }

        private async void ShowLoadErrorDialog(Exception exception)
        {
            await _dialogCoordinator.ShowMessageAsync(this,
                "Loading error.",
                exception.Message != null
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

        [UsedImplicitly]
        public async void SaveMenuItemClicked(object sender, RoutedEventArgs e)
        {
            await SaveMenuItemClicked();
        }

        public async Task SaveMenuItemClicked()
        {
            if (_saveData.IsSavingWithResults == null || _saveData.FilePath == null)
            {
                await SaveTypeChooserDialog();
                return;
            }

            var dataSaver = new XMCDAExporter(_saveData.FilePath, new List<Criterion>(Criteria.CriteriaCollection),
                new List<Alternative>(Alternatives.AlternativesCollection), Results) { OverwriteFile = true };
            try
            {
                if (_saveData.IsSavingWithResults == true) dataSaver.saveSession();
                else dataSaver.saveInput();
            }
            catch (Exception exception)
            {
                ShowSaveErrorDialog(exception);
            }
        }

        public async void SaveAsMenuItemClicked(object sender = null, RoutedEventArgs e = null)
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
            
            await TryToSave(false, dataSaver, directoryPath);
        }

        public async void SaveWithResultsAsMenuItemClicked(object sender = null, RoutedEventArgs e = null)
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

            await TryToSave(true, dataSaver, directoryPath);
        }

        private async Task TryToSave(bool shouldSaveWithResults, XMCDAExporter dataSaver, string directoryPath)
        {
            try
            {
                if (shouldSaveWithResults) dataSaver.saveSession();
                else dataSaver.saveInput();
                _saveData.IsSavingWithResults = shouldSaveWithResults;
                _saveData.FilePath = directoryPath;
            }
            catch (XmcdaFileExistsException)
            {
                var dialogResult = await _dialogCoordinator.ShowMessageAsync(this,
                    "Overwriting files.",
                    "Some XMCDA files already exist in this directory. Would you like to overwrite them?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Yes",
                        NegativeButtonText = "Cancel",
                        DefaultButtonFocus = MessageDialogResult.Negative,
                        AnimateShow = false,
                        AnimateHide = false
                    });
                if (dialogResult == MessageDialogResult.Affirmative)
                {
                    dataSaver.OverwriteFile = true;
                    try
                    {
                        if (shouldSaveWithResults) dataSaver.saveSession();
                        else dataSaver.saveInput();
                        _saveData.IsSavingWithResults = shouldSaveWithResults;
                        _saveData.FilePath = directoryPath;
                    }
                    catch (Exception exception)
                    {
                        ShowSaveErrorDialog(exception);
                    }
                }
            }
        }

        private async void ShowSaveErrorDialog(Exception exception)
        {
            await _dialogCoordinator.ShowMessageAsync(this,
                "Saving error.",
                exception.Message != null
                    ? $"Can't save files. An error was encountered with a following message:\n\"{exception.Message}\""
                    : "Can't save files.",
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    AnimateShow = false,
                    AnimateHide = false,
                    DefaultButtonFocus = MessageDialogResult.Affirmative
                });
        }

        private void InstancePropertyChanged(object sender = null, EventArgs e = null)
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