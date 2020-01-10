using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class ReferenceRankingTab : UserControl
    {
        public ReferenceRankingTab()
        {
            InitializeComponent();
        }

        private void AlternativeDroppedOnNewRank(object sender, DragEventArgs dragEventArgs)
        {
            ((ICollection<Alternative>) ((ItemsControl) sender).ItemsSource).Clear();
        }
    }
}