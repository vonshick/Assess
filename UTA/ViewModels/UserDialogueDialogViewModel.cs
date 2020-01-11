using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using UTA.Annotations;
using UTA.Interactivity;

namespace UTA.ViewModels
{
    public class UserDialogueDialogViewModel : INotifyPropertyChanged
    {
        private readonly Criterion _criterion;

        private readonly Dialog _dialog;
        private readonly int _method;
        private bool _closeDialog;
        private string _textOptionSure;
        private string _textOptionLottery;

        public UserDialogueDialogViewModel(Dialog dialog, Criterion criterion, int method)
        {
            _dialog = dialog;
            _criterion = criterion;
            _method = method;
            Title = "Partial utility function - " + criterion.Name;
            //todo remove, its command line
            dialog.setInitialValues();
            dialog.displayDialog();
            SetChoiceOptionsTextBlocks(dialog);

            TakeSureCommand = new RelayCommand(_ =>
            {
                Console.WriteLine("sure");
                dialog.ProcessDialog(1);
                SetChoiceOptionsTextBlocks(dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                Console.WriteLine("lottery");
                dialog.ProcessDialog(2);
                SetChoiceOptionsTextBlocks(dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                dialog.ProcessDialog(3);
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

        private void SetChoiceOptionsTextBlocks(Dialog dialog)
        {
            if (_method == 3)
            {
                TextOptionSure = "If you prefer a lottery which gives you " + dialog.DisplayObject.ComparisonLottery.UpperUtilityValue.X +
                                    " with probability " + dialog.DisplayObject.ComparisonLottery.P +
                                    " or " + dialog.DisplayObject.ComparisonLottery.LowerUtilityValue.X + " with probability " +
                                    (1 - dialog.DisplayObject.ComparisonLottery.P) + ", click Lottery 1";
                TextOptionLottery = "If you prefer a lottery which gives you " + dialog.DisplayObject.EdgeValuesLottery.UpperUtilityValue.X +
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