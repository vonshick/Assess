using System;
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

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private CoefficientAssessmentTabViewModel _coefficientAssessmentTabViewModel;
        private List<Alternative> _currentCalculationAlternativesCopy;
        private List<Criterion> _currentCalculationCriteriaCopy;
        private SaveData _saveData;
        private ITab _tabToSelect;
        public readonly ObservableCollection<PartialUtilityTabViewModel> PartialUtilityTabViewModels;

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            Criteria = new Criteria();
            Alternatives = new Alternatives(Criteria);
            Results = new Results();

            _dialogCoordinator = dialogCoordinator;
            _saveData = new SaveData(null, null);

            Tabs = new ObservableCollection<ITab>();
            Tabs.CollectionChanged += TabsCollectionChanged;
            ShowTabCommand = new RelayCommand(tabViewModel => ShowTab((ITab) tabViewModel));

            CriteriaTabViewModel = new CriteriaTabViewModel(Criteria);
            AlternativesTabViewModel = new AlternativesTabViewModel(Criteria, Alternatives);
            SettingsTabViewModel = new SettingsTabViewModel();
            WelcomeTabViewModel = new WelcomeTabViewModel();

            PartialUtilityTabViewModels = new ObservableCollection<PartialUtilityTabViewModel>();

            Criteria.CriteriaCollection.CollectionChanged += InstancePropertyChanged;
            Alternatives.AlternativesCollection.CollectionChanged += InstancePropertyChanged;
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
            Results.FinalRanking.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Results.FinalRanking.FinalRankingCollection)) return;
                InstancePropertyChanged();
                Results.FinalRanking.FinalRankingCollection.CollectionChanged += InstancePropertyChanged;
            };

            // TODO: remove. for testing purposes
            // WARNING: using these crashes application at some point
            Criteria.CriteriaCollection.Add(new Criterion("Price", "", "Cost"));
            Criteria.CriteriaCollection.Add(new Criterion("Time", "", "Cost"));
            Criteria.CriteriaCollection.Add(new Criterion("Comfort", "", "Gain"));

            Criteria.CriteriaCollection[0].MinValue = 2;
            Criteria.CriteriaCollection[0].MaxValue = 30;
            Criteria.CriteriaCollection[1].MinValue = 10;
            Criteria.CriteriaCollection[1].MaxValue = 40;
            Criteria.CriteriaCollection[2].MinValue = 0;
            Criteria.CriteriaCollection[2].MaxValue = 3;

            Alternatives.AlternativesCollection.Add(new Alternative("RER", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Metro 1", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Metro 2", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Bus", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Taxi", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Car", Criteria.CriteriaCollection));
            Alternatives.AlternativesCollection.Add(new Alternative("Train", Criteria.CriteriaCollection));

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
        }

        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public Results Results { get; set; }

        public ObservableCollection<ITab> Tabs { get; }
        public RelayCommand ShowTabCommand { get; }

        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }
        public WelcomeTabViewModel WelcomeTabViewModel { get; }


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
                    "It's required to provide data to every criterion value to all alternatives to begin Assess calculations.");
                ShowTab(AlternativesTabViewModel);
                return;
            }

            if (!await IsCriteriaValuesPrecisionAcceptable(Criteria.CriteriaCollection)) return;

            if (_coefficientAssessmentTabViewModel != null || PartialUtilityTabViewModels.Count > 0)
                if (await ShowLosingProgressWarning() == MessageDialogResult.Canceled)
                    return;

            Tabs.Remove(_coefficientAssessmentTabViewModel);
            foreach (var partialUtilityTabViewModel in PartialUtilityTabViewModels) Tabs.Remove(partialUtilityTabViewModel);
            foreach (var partialUtilityTabViewModel in PartialUtilityTabViewModels) Tabs.Remove(partialUtilityTabViewModel);
            PartialUtilityTabViewModels.Clear();

            _currentCalculationCriteriaCopy = Criteria.GetDeepCopyOfCriteria();
            _currentCalculationAlternativesCopy = Alternatives.GetDeepCopyOfAlternatives();

            _coefficientAssessmentTabViewModel = new CoefficientAssessmentTabViewModel(_currentCalculationCriteriaCopy, ShowPartialUtilityTabs);
            ShowTab(_coefficientAssessmentTabViewModel);
        }

        private void AddTabIfNeeded(ITab tab)
        {
            if (!Tabs.Contains(tab)) Tabs.Add(tab);
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

        private async Task<MessageDialogResult> ShowLosingProgressWarning()
        {
            var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, "Losing current progress.",
                "Your current calculations progress will be lost.\n" +
                "If you accidentally closed some tabs, you can show them again by using Show menu option.\n" +
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
            return dialogResult;
        }

        private async Task<bool> IsCriteriaValuesPrecisionAcceptable(IEnumerable<Criterion> criteriaList)
        {
            var invalidCriteriaValuesNames = new List<string>();
            foreach (var criterion in criteriaList)
                if (Math.Abs(criterion.MaxValue - criterion.MinValue) < 1E-14)
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
                return false;
            }

            return true;
        }

        // called in CoefficientAssessmentTabViewModel, after clicking Indifferent in last dialogue question
        private void ShowPartialUtilityTabs(List<CriterionCoefficient> criteriaCoefficients)
        {
            Results.CriteriaCoefficients = criteriaCoefficients;

            Tabs.Remove(_coefficientAssessmentTabViewModel);

            var utilitiesCalculator =
                new UtilitiesCalculator(_currentCalculationAlternativesCopy, Results, _currentCalculationCriteriaCopy);
            utilitiesCalculator.CalculateGlobalUtilities();

            foreach (var partialUtility in Results.PartialUtilityFunctions)
            {
                var partialUtilityTabViewModel =
                    new PartialUtilityTabViewModel(partialUtility, utilitiesCalculator.CalculateGlobalUtilities);
                PartialUtilityTabViewModels.Add(partialUtilityTabViewModel);
                Tabs.Add(partialUtilityTabViewModel);
            }

            if (PartialUtilityTabViewModels.Count > 0) ShowTab(PartialUtilityTabViewModels[0]);
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

            // TODO: clear every data
            Results.Reset();
            Alternatives.Reset();
            Criteria.Reset();
            //foreach (var chartTabViewModel in ChartTabViewModels)
            //    Tabs.Remove(chartTabViewModel);
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
                // TODO: utx in Assess?
                Filter = "Assess Input Files (*.xml; *.csv; *.utx)|*.xml;*.csv;*.utx",
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
                // works assuming that CriteriaValuesList are initialized properly
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
                Results.PartialUtilityFunctions = dataLoader.Results.PartialUtilityFunctions;
                if (Results.PartialUtilityFunctions.Count <= 0) return;
                // TODO: load state
                //var alternativesDeepCopy = Alternatives.GetDeepCopyOfAlternatives();

                //foreach (var partialUtility in Results.PartialUtilityFunctions)
                //{
                //    var viewModel = new ChartTabViewModel(_solver, partialUtility, SettingsTabViewModel, RefreshCharts);
                //    ChartTabViewModels.Add(viewModel);
                //    Tabs.Add(viewModel);
                //}

                //if (ChartTabViewModels.Count > 0) ShowTab(ChartTabViewModels[0]);
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