using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class UserDialogueDialog
    {
        private UserDialogueDialogViewModel _viewmodel;
        private readonly IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;
        
        public UserDialogueDialog()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (UserDialogueDialogViewModel) DataContext;
            _viewmodel.DialogCoordinator = dialogCoordinator;
            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName != nameof(_viewmodel.CloseDialog)) return;
                Console.WriteLine("Close prop changed");
                Close();
            };
        }

        private void DialogClosed(object sender, CancelEventArgs e)
        {
            // cancel close, because window doesn't wait for async function and closes anyway
            e.Cancel = !_viewmodel.CloseDialog;
            _viewmodel.DialogClosed();
        }
    }
}