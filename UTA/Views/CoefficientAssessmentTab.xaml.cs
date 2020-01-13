using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class CoefficientAssessmentDialog
    {
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;
        private CoefficientAssessmentDialogViewModel _viewmodel;

        public CoefficientAssessmentDialog()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (CoefficientAssessmentDialogViewModel) DataContext;
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