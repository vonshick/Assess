using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using DataModel.Results;
using ExportModule;
using ImportModule;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using UTA.Annotations;
using UTA.Helpers;
using UTA.Models;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private bool _preserveKendallCoefficient;
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
            PartialUtilityTabViewModels = new ObservableCollection<PartialUtilityTabViewModel>();

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            ReferenceRankingTabViewModel = new ReferenceRankingTabViewModel(ReferenceRanking, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
            WelcomeTabViewModel = new WelcomeTabViewModel();

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
            /*
            Criteria.AddCriterion("Price", "", "Cost", 2);
            Criteria.AddCriterion("Time", "", "Cost", 3);
            Criteria.AddCriterion("Comfort", "", "Gain", 3);

            Criteria.CriteriaCollection[0].MinValue = 2;
            Criteria.CriteriaCollection[0].MaxValue = 30;
            Criteria.CriteriaCollection[1].MinValue = 10;
            Criteria.CriteriaCollection[1].MaxValue = 40;
            Criteria.CriteriaCollection[2].MinValue = 0;
            Criteria.CriteriaCollection[2].MaxValue = 3;

            Alternatives.AddAlternative("RER", "");
            Alternatives.AddAlternative("METRO (1)", "");
            Alternatives.AddAlternative("METRO (2)", "");
            Alternatives.AddAlternative("BUS", "");
            Alternatives.AddAlternative("TAXI", "");
            Alternatives.AddAlternative("Other1", "");
            Alternatives.AddAlternative("Other2", "");

            Alternatives.AlternativesCollection[0].CriteriaValuesList[0].Value = 3;
            Alternatives.AlternativesCollection[0].CriteriaValuesList[1].Value = 10;
            Alternatives.AlternativesCollection[0].CriteriaValuesList[2].Value = 1;
            Alternatives.AlternativesCollection[1].CriteriaValuesList[0].Value = 4;
            Alternatives.AlternativesCollection[1].CriteriaValuesList[1].Value = 20;
            Alternatives.AlternativesCollection[1].CriteriaValuesList[2].Value = 2;
            Alternatives.AlternativesCollection[2].CriteriaValuesList[0].Value = 2;
            Alternatives.AlternativesCollection[2].CriteriaValuesList[1].Value = 20;
            Alternatives.AlternativesCollection[2].CriteriaValuesList[2].Value = 0;
            Alternatives.AlternativesCollection[3].CriteriaValuesList[0].Value = 6;
            Alternatives.AlternativesCollection[3].CriteriaValuesList[1].Value = 40;
            Alternatives.AlternativesCollection[3].CriteriaValuesList[2].Value = 0;
            Alternatives.AlternativesCollection[4].CriteriaValuesList[0].Value = 30;
            Alternatives.AlternativesCollection[4].CriteriaValuesList[1].Value = 30;
            Alternatives.AlternativesCollection[4].CriteriaValuesList[2].Value = 3;
            Alternatives.AlternativesCollection[5].CriteriaValuesList[0].Value = 8;
            Alternatives.AlternativesCollection[5].CriteriaValuesList[1].Value = 24;
            Alternatives.AlternativesCollection[5].CriteriaValuesList[2].Value = 2;
            Alternatives.AlternativesCollection[6].CriteriaValuesList[0].Value = 16;
            Alternatives.AlternativesCollection[6].CriteriaValuesList[1].Value = 36;
            Alternatives.AlternativesCollection[6].CriteriaValuesList[2].Value = 3;

            ReferenceRanking.AddAlternativeToRank(Alternatives.AlternativesCollection[0], 1);
            ReferenceRanking.AddAlternativeToRank(Alternatives.AlternativesCollection[1], 2);
            ReferenceRanking.AddAlternativeToRank(Alternatives.AlternativesCollection[2], 2);
            ReferenceRanking.AddAlternativeToRank(Alternatives.AlternativesCollection[3], 3);
            ReferenceRanking.AddAlternativeToRank(Alternatives.AlternativesCollection[4], 4);
            */

            //Criteria.AddCriterion("0-100 km/h", "", "Cost", 5);
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
            //        0.1919191919f));
            //Results.KendallCoefficient = 0.191919191919f;

            // TODO vonshick REMOVE IT AFTER TESTING
//               string dataDirectoryPath = "D:\\Data";
//               DataLoader dataLoader = SampleImport.ProcessSampleData(dataDirectoryPath); // csv
////               SampleExport.exportXMCDA(dataDirectoryPath, dataLoader.CriterionList, dataLoader.AlternativeList);
//
////               CoefficientsDialog coefficientsDialog = new CoefficientsDialog(dataLoader.CriterionList);
////               coefficientsDialog.GetCoefficientsForCriteria();
////               List<CriterionCoefficient> criteriaCoefficientsList = coefficientsDialog.CriteriaCoefficientsList;
//
//               List<CriterionCoefficient> criteriaCoefficientsList = new List<CriterionCoefficient>();
//               List<PartialUtility> partialUtilitiesList = new List<PartialUtility>();
//               DialogController dialogController;
//
//               foreach (Criterion criterion in dataLoader.CriterionList)
//               {
//                   dialogController = new DialogController(criterion, 1, 0.3f); // 1. criterion object 2. Dialog type (integer from 1 to 4 - description in DialogController) 3. P
//                   dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]); // ends of the selected segment
//                   partialUtilitiesList.Add(new PartialUtility(criterion, dialogController.DisplayObject.PointsList));
//                   criteriaCoefficientsList.Add(new CriterionCoefficient(criterion.Name, 0.5f)); // mock for coefficients which we can get from CoefficientsDialog - example a few lines above
//               }
//
//               var utilitiesCalculator = new UtilitiesCalculator(dataLoader.AlternativeList, dataLoader.CriterionList, partialUtilitiesList, criteriaCoefficientsList);
//               utilitiesCalculator.CalculateGlobalUtilities();
//               FinalRankingAssess finalRankingAssess = new FinalRankingAssess(utilitiesCalculator.AlternativesUtilitiesList);
        }

        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public ReferenceRanking ReferenceRanking { get; set; }
        public Results Results { get; set; }

        public ObservableCollection<ITab> Tabs { get; }
        public ObservableCollection<ChartTabViewModel> ChartTabViewModels { get; }
        public ObservableCollection<PartialUtilityTabViewModel> PartialUtilityTabViewModels { get; }
        public RelayCommand ShowTabCommand { get; }

        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public ReferenceRankingTabViewModel ReferenceRankingTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }
        public WelcomeTabViewModel WelcomeTabViewModel { get; }


        public bool PreserveKendallCoefficient
        {
            get => _preserveKendallCoefficient;
            set
            {
                if (_preserveKendallCoefficient == value) return;
                _preserveKendallCoefficient = value;
                _solver?.UpdatePreserveKendallCoefficient(_preserveKendallCoefficient);
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
        public void CalculateButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Criteria.CriteriaCollection.Count == 0)
            {
                ShowCalculateErrorDialog("It's required to provide at least 1 criterion to begin Assess calculations.");
                AddTabIfNeeded(CriteriaTabViewModel);
                AddTabIfNeeded(AlternativesTabViewModel);
                ShowTab(CriteriaTabViewModel);
                return;
            }

            if (Alternatives.AlternativesCollection.Count <= 1)
            {
                ShowCalculateErrorDialog("It's required to provide at least 2 alternatives to begin Assess calculations.");
                AddTabIfNeeded(AlternativesTabViewModel);
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

            if (!CriteriaValuesCorrect(Criteria.CriteriaCollection.ToList())) return;

            //todo deep copies
            var copyAlternatives = Alternatives.AlternativesCollection.ToList();
            var copyCriteria = Criteria.CriteriaCollection.ToList();

            var criteriaCoefficientsList = AssessCoefficients(copyCriteria);
            if (criteriaCoefficientsList == null) return;

            //get initial partial utilities
            var partialUtilitiesList = new List<PartialUtility>();
            foreach (var criterion in copyCriteria)
                partialUtilitiesList.Add(new PartialUtility(criterion, new DialogController(criterion, 1, 0.5f).DisplayObject.PointsList));

            //run solver for initial utilities
            var utilitiesCalculator = new UtilitiesCalculator(copyAlternatives, partialUtilitiesList, criteriaCoefficientsList);
            utilitiesCalculator.CalculateGlobalUtilities();

            //todo present in panels
            var finalRankingAssess = new FinalRankingAssess(utilitiesCalculator.AlternativesUtilitiesList);

            ShowPartialUtilityTabs(copyCriteria);

            //todo after first calculation - dynamic recalcualtion
        }

        private bool CriteriaValuesCorrect(List<Criterion> criteriaList)
        {
            var invalidCriteriaValuesNames = new List<string>();
            foreach (var criterion in criteriaList)
                if (Math.Abs(criterion.MaxValue - criterion.MinValue) < 0.000000001)
                    invalidCriteriaValuesNames.Add(criterion.Name);

            if (invalidCriteriaValuesNames.Count != 0)
            {
                ShowTab(AlternativesTabViewModel);
                ShowCriteriaMinMaxValueWarning(invalidCriteriaValuesNames);
                return false;
            }

            return true;
        }

        private List<CriterionCoefficient> AssessCoefficients(List<Criterion> criteriaList)
        {
            var coefficientTabViewModel = new CoefficientAssessmentDialogViewModel(criteriaList);
            var coefficientAssessmentDialog = new CoefficientAssessmentDialog {DataContext = coefficientTabViewModel};
            coefficientAssessmentDialog.ShowDialog();
            return coefficientTabViewModel.CriteriaCoefficientsList;
        }

        private void ShowPartialUtilityTabs(List<Criterion> criteriaList)
        {
            foreach (var partialUtilityTabViewModel in PartialUtilityTabViewModels) Tabs.Remove(partialUtilityTabViewModel);
            PartialUtilityTabViewModels.Clear();

            foreach (var criterion in criteriaList)
            {
                var partialUtilityTabViewModel = new PartialUtilityTabViewModel(criterion);
                PartialUtilityTabViewModels.Add(partialUtilityTabViewModel);
                Tabs.Add(partialUtilityTabViewModel);
                if (PartialUtilityTabViewModels.Count > 0) ShowTab(PartialUtilityTabViewModels[0]);
            }
        }

        private void AddTabIfNeeded(ITab tab)
        {
            if (!Tabs.Contains(tab)) Tabs.Add(tab);
        }

        private async void ShowCriteriaMinMaxValueWarning(List<string> invalidCriteriaValuesNames)
        {
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
        }


        private void RefreshCharts()
        {
            // TODO: optional. consider using faster method than GenerateChartData
            foreach (var chartTabViewModel in ChartTabViewModels) chartTabViewModel.GenerateChartData();
        }

        private async void ShowCalculateErrorDialog(string message, string title = "Incomplete instance data.")
        {
            await _dialogCoordinator.ShowMessageAsync(this,
                title,
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
                "Your progress will be lost. Would you like to proceed without saving?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "Save",
                    FirstAuxiliaryButtonText = "Cancel",
                    DialogResultOnCancel = MessageDialogResult.FirstAuxiliary,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (saveDialogResult == MessageDialogResult.FirstAuxiliary) return false;
            if (saveDialogResult == MessageDialogResult.Negative)
                await SaveTypeChooserDialog();

            _solver = null;
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
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Save With Results",
                    NegativeButtonText = "Save Without Results",
                    FirstAuxiliaryButtonText = "Cancel",
                    DialogResultOnCancel = MessageDialogResult.FirstAuxiliary,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (saveFileWithResultsDialog == MessageDialogResult.Affirmative) SaveWithResultsAsMenuItemClicked();
            else if (saveFileWithResultsDialog == MessageDialogResult.Negative) SaveAsMenuItemClicked();
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
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                // works assuming that CriteriaValuesList and ReferenceRank property are initialized properly
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
                Results.PartialUtilityFunctions = dataLoader.Results.PartialUtilityFunctions;
                if (Results.PartialUtilityFunctions.Count <= 0) return;
                var alternativesDeepCopy = Alternatives.GetDeepCopyOfAlternatives();
                var alternativesWithoutRanksCopy = alternativesDeepCopy.Where(alternative => alternative.ReferenceRank == null).ToList();
                var referenceRankingDeepCopy = ReferenceRanking.GetDeepCopyOfReferenceRanking(alternativesDeepCopy);
                _solver = new Solver(
                    referenceRankingDeepCopy,
                    Criteria.GetDeepCopyOfCriteria(),
                    alternativesWithoutRanksCopy,
                    Results,
                    PreserveKendallCoefficient,
                    SettingsTabViewModel.DeltaThreshold,
                    SettingsTabViewModel.EpsilonThreshold);
                _solver.LoadState(Results.PartialUtilityFunctions, referenceRankingDeepCopy, alternativesWithoutRanksCopy, Results);
                foreach (var partialUtility in Results.PartialUtilityFunctions)
                {
                    var viewModel = new ChartTabViewModel(_solver, partialUtility, SettingsTabViewModel, RefreshCharts);
                    ChartTabViewModels.Add(viewModel);
                    Tabs.Add(viewModel);
                }

                if (ChartTabViewModels.Count > 0) ShowTab(ChartTabViewModels[0]);
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
                new List<Alternative>(Alternatives.AlternativesCollection), Results) {OverwriteFile = true};
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