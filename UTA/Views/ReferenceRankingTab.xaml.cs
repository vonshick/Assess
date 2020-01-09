using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataModel.Input;
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
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (ReferenceRankingTabViewModel) DataContext;
        }

        private void AlternativeDroppedOnNewRank(object sender, DragEventArgs dragEventArgs)
        {
            ((ICollection<Alternative>) ((ItemsControl) sender).ItemsSource).Clear();
        }
    }
}