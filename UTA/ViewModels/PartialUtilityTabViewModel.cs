using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using UTA.Annotations;
using UTA.Interactivity;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class PartialUtilityTabViewModel : Tab, INotifyPropertyChanged
    {
        private string _resultsText;
        private string _textOptionLottery;
        private string _textOptionSure;
        private bool _utilityAssessed;


        public PartialUtilityTabViewModel(Criterion criterion)
        {
            Criterion = criterion;
            Name = "Utility - " + criterion.Name;
            ResultsText = "Results will be here after finished dialogue";
            StartButtonText = "Start Assessment";

            StartDialogueCommand = new RelayCommand(_ =>
            {
                StartButtonText = "Restart Assessment";
                ShowMethodDialog();
                Console.WriteLine(Criterion.MinValue + ", max: " + Criterion.MaxValue);
                //todo what probability?
                DialogController = new DialogController(Criterion, Method, 0.5f);
                Dialog = DialogController.TriggerDialog(DialogController.PointsList[0], DialogController.PointsList[1]);
                Dialog.SetInitialValues();
                SetUtilityAssessmentTextBlocks(Dialog);
            });
            TakeSureCommand = new RelayCommand(_ =>
            {
                Dialog.ProcessDialog(1);
                SetUtilityAssessmentTextBlocks(Dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                Dialog.ProcessDialog(2);
                SetUtilityAssessmentTextBlocks(Dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                Dialog.ProcessDialog(3);
                UtilityAssessed = true;
                DigestResults(Dialog);
            });
        }

        public int Method { get; set; }

        private Dialog Dialog { get; set; }

        private DialogController DialogController { get; set; }

        public Criterion Criterion { get; }
        public RelayCommand StartDialogueCommand { get; }
        public RelayCommand TakeSureCommand { get; }
        public RelayCommand TakeLotteryCommand { get; }
        public RelayCommand TakeIndifferentCommand { get; }
        public string StartButtonText { get; set; }


        public bool UtilityAssessed
        {
            get => _utilityAssessed;
            set
            {
                if (value == _utilityAssessed) return;
                _utilityAssessed = value;
                OnPropertyChanged(nameof(UtilityAssessed));
            }
        }

        public string ResultsText
        {
            get => _resultsText;
            set
            {
                if (value == _resultsText) return;
                _resultsText = value;
                OnPropertyChanged();
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void ShowMethodDialog()
        {
            Console.WriteLine("Showing dialog for " + Criterion.Name);
            var methodDialogViewModel = new AssessmentMethodDialogViewModel(Criterion.Name);
            var assessmentMethodDialog = new AssessmentMethodDialog();
            assessmentMethodDialog.DataContext = methodDialogViewModel;
            assessmentMethodDialog.ShowDialog();
            Method = methodDialogViewModel.Method;
        }

        private void SetUtilityAssessmentTextBlocks(Dialog dialog)
        {
            if (Method == 3)
            {
                TextOptionSure = "Criterion " + Criterion.Name + "\nIf you prefer a lottery which gives you " + dialog.DisplayObject.ComparisonLottery.UpperUtilityValue.X +
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
                TextOptionSure = "Criterion " + Criterion.Name + "\nIf you prefer to have " + dialog.DisplayObject.X + " for sure, click Take Sure.";
                TextOptionLottery = "If you prefer a lottery which gives you " + dialog.DisplayObject.Lottery.UpperUtilityValue.X +
                                    " with probability " + dialog.DisplayObject.Lottery.P +
                                    " or " + dialog.DisplayObject.Lottery.LowerUtilityValue.X + " with probability " +
                                    (1 - dialog.DisplayObject.Lottery.P + ", click Take Lottery");
            }
        }

        //todo temporary to show result
        private void DigestResults(Dialog dialog)
        {
            ResultsText = "";

            if (Method != 3) ResultsText = "wsp. równoważności = " + dialog.DisplayObject.X + ", ";
            ResultsText += "punkty:\n";
            foreach (var point in dialog.DisplayObject.PointsList) ResultsText += "(" + point.X + ";" + point.Y + ")\n";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}