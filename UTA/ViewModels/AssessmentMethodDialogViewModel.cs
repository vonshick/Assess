using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MahApps.Metro.Controls.Dialogs;
using UTA.Annotations;
using UTA.Interactivity;

namespace UTA.ViewModels
{
    public class AssessmentMethodDialogViewModel : INotifyPropertyChanged
    {
        private bool _closeDialog;

        public AssessmentMethodDialogViewModel(string criterionName)
        {
            Title = "Select Assessment Method for " + criterionName;
            PromptText = "Please select assessment method for " + criterionName + ":";
            ButtonOkClickedCommand = new RelayCommand(_ =>
            {
                Console.WriteLine(Method);
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

        public RelayCommand ButtonOkClickedCommand { get; }
        public IDialogCoordinator DialogCoordinator { get; set; }
        public int Method { get; set; }
        public string Title { get; set; }
        public string PromptText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void DialogClosed()
        {
            var dialogResult = await DialogCoordinator.ShowMessageAsync(this,
                "Cannot close dialog!",
                "You have to select method.",
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    AnimateShow = false,
                    AnimateHide = false
                });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}