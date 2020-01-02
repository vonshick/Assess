using System.Windows.Controls;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsTabViewModel _viewmodel;

        public SettingsTab()
        {
            InitializeComponent();
            _viewmodel = ((MainViewModel) DataContext).SettingsTabViewModel;
        }
    }
}