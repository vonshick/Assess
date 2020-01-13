using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class AssessmentMethodDialog
    {
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;
        private AssessmentMethodDialogViewModel _viewmodel;

        public AssessmentMethodDialog()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        //todo selection logic and showing this dialog

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (AssessmentMethodDialogViewModel) DataContext;
            _viewmodel.DialogCoordinator = _dialogCoordinator;
            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName != nameof(_viewmodel.CloseDialog)) return;
                Close();
            };
        }

        private void DialogClosed(object sender, CancelEventArgs e)
        {
            _viewmodel.DialogClosed(sender, e);
        }
    }
}