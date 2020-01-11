using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using UTA.Annotations;
using UTA.Interactivity;

namespace UTA.ViewModels
{
    public class UserDialogueDialogViewModel : INotifyPropertyChanged
    {
        private readonly int _method;
        private bool _closeDialog;
        private string _textOptionLottery;
        private string _textOptionSure;

        public UserDialogueDialogViewModel(Dialog dialog, Criterion criterion, int method)
        {
            _method = method;
            Title = "Partial utility function - " + criterion.Name;
            dialog.SetInitialValues();
            SetUtilityAssessmentTextBlocks(dialog);

            TakeSureCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(1);
                SetUtilityAssessmentTextBlocks(dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(2);
                SetUtilityAssessmentTextBlocks(dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(3);
                CloseDialog = true;
            });
        }

        public UserDialogueDialogViewModel(CoefficientsDialog dialog, Criterion criterion)
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
                CloseDialog = true;
            });
        }

        public bool CloseDialog
        {
            get => _closeDialog;
            set
            {
                _closeDialog = value;
                OnPropertyChanged(nameof(CloseDialog));
            }
        }

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

        public string Title { get; set; }

        public RelayCommand TakeSureCommand { get; }
        public RelayCommand TakeLotteryCommand { get; }
        public RelayCommand TakeIndifferentCommand { get; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void SetCoefficientsTextBlocks(CoefficientsDialog dialog)
        {
            TextOptionSure = "Click 'Sure' if you prefer to have for sure:\n";
            for (int i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
            {
                TextOptionSure += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.ValuesToCompare[i] + "\n";
            }

            TextOptionLottery += "\nClick 'Lottery' if you prefer to have with probability " + dialog.DisplayObject.P + " these values:\n";

            for (int i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
            {
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.BestValues[i] + "\n";
            }

            TextOptionLottery += "\nOR with probability " + dialog.DisplayObject.P + " these values:\n";

            for (int i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
            {
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.WorstValues[i] + "\n";
            }
        }

        private void SetUtilityAssessmentTextBlocks(Dialog dialog)
        {
            if (_method == 3)
            {
                TextOptionSure = "If you prefer a lottery which gives you " + dialog.DisplayObject.ComparisonLottery.UpperUtilityValue.X +
                                 " with probability " + dialog.DisplayObject.ComparisonLottery.P +
                                 " or " + dialog.DisplayObject.ComparisonLottery.LowerUtilityValue.X + " with probability " +
                                 (1 - dialog.DisplayObject.ComparisonLottery.P) + ", click Lottery 1";
                TextOptionLottery = "If you prefer a lottery which gives you " +
                                    dialog.DisplayObject.EdgeValuesLottery.UpperUtilityValue.X +
                                    " with probability " + dialog.DisplayObject.EdgeValuesLottery.P +
                                    " or " + dialog.DisplayObject.EdgeValuesLottery.LowerUtilityValue.X + " with probability " +
                                    (1 - dialog.DisplayObject.EdgeValuesLottery.P) + ", click Lottery 2";
            }
            else
            {
                TextOptionSure = "If you prefer to have " + dialog.DisplayObject.X + " for sure, click Take Sure.";
                TextOptionLottery = "If you prefer a lottery which gives you " + dialog.DisplayObject.Lottery.UpperUtilityValue.X +
                                    " with probability " + dialog.DisplayObject.Lottery.P +
                                    " or " + dialog.DisplayObject.Lottery.LowerUtilityValue.X + " with probability " +
                                    (1 - dialog.DisplayObject.Lottery.P + ", click Take Lottery");
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}