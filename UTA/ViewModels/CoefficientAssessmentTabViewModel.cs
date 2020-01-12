using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using MahApps.Metro.Controls.Dialogs;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class CoefficientAssessmentTabViewModel : Tab, INotifyPropertyChanged
    {
        private string _textOptionSure;
        private string _textOptionLottery;

        public CoefficientAssessmentTabViewModel(CoefficientsDialog dialog, Criterion criterion)
        {
            Title = "Scaling coefficient- " + criterion.Name;
            dialog.SetInitialValues(criterion);
            SetCoefficientsTextBlocks(dialog);

            TakeSureCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(1);
                SetCoefficientsTextBlocks(dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(2);
                SetCoefficientsTextBlocks(dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(3);
                dialog.GetCoefficientsForCriterion(criterion);
                UtilityAssessed = true;
            });
        }

        public bool UtilityAssessed { get; set; }
        public string Title { get; set; }
        public RelayCommand TakeSureCommand { get; }
        public RelayCommand TakeLotteryCommand { get; }
        public RelayCommand TakeIndifferentCommand { get; } 
        public IDialogCoordinator DialogCoordinator { get; set; }
        public string TextOptionSure
        {
            get => _textOptionSure;
            set
            {
                _textOptionSure = value;
                OnPropertyChanged(nameof(TextOptionSure));
            }
        }
        public string TextOptionLottery
        {
            get => _textOptionLottery;
            set
            {
                _textOptionLottery = value;
                OnPropertyChanged(nameof(TextOptionLottery));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        private void SetCoefficientsTextBlocks(CoefficientsDialog dialog)
        {
            TextOptionSure = "Click 'Sure' if you prefer to have for sure:\n";
            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionSure += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.ValuesToCompare[i] + "\n";

            TextOptionLottery += "\nClick 'Lottery' if you prefer to have with probability " + dialog.DisplayObject.P + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.BestValues[i] + "\n";

            TextOptionLottery += "\nOR with probability " + dialog.DisplayObject.P + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.WorstValues[i] + "\n";
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void DialogClosed()
        {
            var dialogResult = await DialogCoordinator.ShowMessageAsync(this,
                "Loosing assessment progress!",
                "Assessment progress will be lost. Do you want to close the window anyway?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    DefaultButtonFocus = MessageDialogResult.Negative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                UtilityAssessed = false;
                RestoreUtilityFunction();
            }
        }

        public void RestoreUtilityFunction()
        {
            Console.WriteLine("Restoring utility function");
            //todo restore utility function when user didn't finish assessment and closed dialog
        }
    }
}