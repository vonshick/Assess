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
        private int _method;
        private string _resultsText;
        private bool _utilityAssessed;

        public PartialUtilityTabViewModel(Criterion criterion)
        {
            Criterion = criterion;
            Name = "Utility - " + criterion.Name;
            StartButtonEnabled = true;
            ResultsText = "Results will be here after finished dialogue";

            StartDialogueCommand = new RelayCommand(_ =>
            {
                //todo ask for method first
                _method = int.Parse(Method);
                //todo remove
                Criterion.MinValue = float.Parse(StartPoint);
                Criterion.MaxValue = float.Parse(EndPoint);
                Console.WriteLine(Criterion.CriterionDirection);
                var dialogController = new DialogController(Criterion, _method, 0.3f);
                var dialog = dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);
                ShowDialogueDialog(dialog, Criterion);
            });
        }

        public Criterion Criterion { get; }
        public RelayCommand StartDialogueCommand { get; }
        public string Method { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public bool StartButtonEnabled { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void ShowDialogueDialog(Dialog dialog, Criterion criterion)
        {
            StartButtonEnabled = false;

            var userDialogueDialogViewModel = new UserDialogueDialogViewModel(dialog, criterion, _method);
            var userDialogueDialog = new UserDialogueDialog {DataContext = userDialogueDialogViewModel};
            if (_method == 3)
            {
                userDialogueDialog.ButtonSure.Content = "Lottery 1";
                userDialogueDialog.ButtonLottery.Content = "Lottery 2";
            }

            userDialogueDialog.ShowDialog();
            UtilityAssessed = userDialogueDialogViewModel.UtilityAssessed;
            StartButtonEnabled = true;
            DigestResults(dialog);
        }

        //todo temporary to show result
        private void DigestResults(Dialog dialog)
        {
            ResultsText = "";

            if (_method != 3) ResultsText = "wsp. równoważności = " + dialog.DisplayObject.X + ", ";
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