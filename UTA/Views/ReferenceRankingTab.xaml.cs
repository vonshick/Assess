using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class ReferenceRankingTab : UserControl
    {
        private ReferenceRankingTabViewModel _viewmodel;

        public ReferenceRankingTab()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
            AlternativesListView.GiveFeedback += OnGiveFeedback;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = ((MainViewModel) DataContext).ReferenceRankingTabViewModel;
        }

        protected void OnGiveFeedback(object o, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
        }

        //todo RELYCOMMAND
        private void AddRankButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewmodel.AddRank();
        }

    }

}