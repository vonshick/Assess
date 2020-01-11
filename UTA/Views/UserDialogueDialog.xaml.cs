using System.Windows;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class UserDialogueDialog
    {
        private UserDialogueDialogViewModel _viewmodel;


        public UserDialogueDialog()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (UserDialogueDialogViewModel) DataContext;
            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName != nameof(_viewmodel.CloseDialog)) return;
                Close();
            };
        }
    }
}