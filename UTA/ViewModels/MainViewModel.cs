using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CalculationsEngine.Maintenance;
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
        public readonly ObservableCollection<PartialUtilityTabViewModel> PartialUtilityTabViewModels;
        private CoefficientAssessmentTabViewModel _coefficientAssessmentTabViewModel;
        private List<Alternative> _currentCalculationAlternativesCopy;
        private List<Criterion> _currentCalculationCriteriaCopy;
        private SaveData _saveData;
        private ITab _tabToSelect;
        private UtilitiesCalculator _utilitiesCalculator;


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
        }

        public Alternatives Alternatives { get; set; }
        public Criteria Criteria { get; set; }
        public Results Results { get; set; }

        public ObservableCollection<ITab> Tabs { get; }
        public RelayCommand ShowTabCommand { get; }

        public CriteriaTabViewModel CriteriaTabViewModel { get; }
        public AlternativesTabViewModel AlternativesTabViewModel { get; }
        public WelcomeTabViewModel WelcomeTabViewModel { get; }

        public CoefficientAssessmentTabViewModel CoefficientAssessmentTabViewModel
        {
            get => _coefficientAssessmentTabViewModel;
            set
            {
                _coefficientAssessmentTabViewModel = value;
                OnPropertyChanged(nameof(CoefficientAssessmentTabViewModel));
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

            if (CoefficientAssessmentTabViewModel != null || PartialUtilityTabViewModels.Count > 0)
                if (await ShowLosingProgressWarning() == MessageDialogResult.Canceled)
                    return;

            Tabs.Remove(CoefficientAssessmentTabViewModel);
            foreach (var partialUtilityTabViewModel in PartialUtilityTabViewModels) Tabs.Remove(partialUtilityTabViewModel);
            PartialUtilityTabViewModels.Clear();

            _currentCalculationCriteriaCopy = Criteria.GetDeepCopyOfCriteria();
            _currentCalculationAlternativesCopy = Alternatives.GetDeepCopyOfAlternatives();

            CoefficientAssessmentTabViewModel =
                new CoefficientAssessmentTabViewModel(_currentCalculationCriteriaCopy, Results, ShowPartialUtilityTabs);
            ShowTab(CoefficientAssessmentTabViewModel);
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
                if (Math.Abs(criterion.MaxValue - criterion.MinValue) < 1E-15)
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

        // called in CoefficientAssessmentTabViewModel after clicking Indifferent in last dialogue question on calculations start
        private void ShowPartialUtilityTabs()
        {
            Tabs.Remove(CoefficientAssessmentTabViewModel);
            CoefficientAssessmentTabViewModel = null;

            _utilitiesCalculator = new UtilitiesCalculator(_currentCalculationAlternativesCopy, Results, _currentCalculationCriteriaCopy);
            _utilitiesCalculator.CalculateGlobalUtilities();

            foreach (var partialUtility in Results.PartialUtilityFunctions)
            {
                var partialUtilityTabViewModel =
                    new PartialUtilityTabViewModel(partialUtility, _utilitiesCalculator.CalculateGlobalUtilities,
                        RestartCoefficientAssessmentDialogue);
                PartialUtilityTabViewModels.Add(partialUtilityTabViewModel);
                Tabs.Add(partialUtilityTabViewModel);
            }

            if (PartialUtilityTabViewModels.Count > 0) ShowTab(PartialUtilityTabViewModels[0]);
        }

        private void RestartCoefficientAssessmentDialogue()
        {
            CoefficientAssessmentTabViewModel =
                new CoefficientAssessmentTabViewModel(_currentCalculationCriteriaCopy, Results, UpdateScalingCoefficients);
            ShowTab(CoefficientAssessmentTabViewModel);
        }

        private void UpdateScalingCoefficients()
        {
            Tabs.Remove(CoefficientAssessmentTabViewModel);
            CoefficientAssessmentTabViewModel = null;
            _utilitiesCalculator?.UpdateScalingCoefficients();
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

            ResetProgress();
            return true;
        }

        private void ResetProgress()
        {
            Results.Reset();
            Alternatives.Reset();
            Criteria.Reset();
            Tabs.Remove(CoefficientAssessmentTabViewModel);
            CoefficientAssessmentTabViewModel = null;
            foreach (var partialUtilityTabViewModel in PartialUtilityTabViewModels) Tabs.Remove(partialUtilityTabViewModel);
            PartialUtilityTabViewModels.Clear();
            _utilitiesCalculator = null;
            _currentCalculationAlternativesCopy?.Clear();
            _currentCalculationCriteriaCopy?.Clear();
            _saveData.IsSavingWithResults = null;
            _saveData.FilePath = null;
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
                if (!await IsCriteriaValuesPrecisionAcceptable(dataLoader.CriterionList)) return;
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
            }
            catch (Exception exception)
            {
                ResetProgress();
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
                if (!await IsCriteriaValuesPrecisionAcceptable(dataLoader.CriterionList)) return;
                Criteria.CriteriaCollection = new ObservableCollection<Criterion>(dataLoader.CriterionList);
                _currentCalculationCriteriaCopy = Criteria.GetDeepCopyOfCriteria();
                // works assuming that CriteriaValuesList are initialized properly
                Alternatives.AlternativesCollection = new ObservableCollection<Alternative>(dataLoader.AlternativeList);
                _currentCalculationAlternativesCopy = Alternatives.GetDeepCopyOfAlternatives();
                Results.CriteriaCoefficients = dataLoader.Results.CriteriaCoefficients;
                if (dataLoader.Results.PartialUtilityFunctions.Count <= 0) return;
                Results.PartialUtilityFunctions = dataLoader.Results.PartialUtilityFunctions;

                ShowPartialUtilityTabs();
            }
            catch (Exception exception)
            {
                ResetProgress();
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

        // Task return type isn't allowed in XAML
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
            var dataSaver = new XMCDAExporter(
                directoryPath,
                _currentCalculationCriteriaCopy ?? new List<Criterion>(Criteria.CriteriaCollection),
                _currentCalculationAlternativesCopy ?? new List<Alternative>(Alternatives.AlternativesCollection),
                Results);

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