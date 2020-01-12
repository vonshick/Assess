using System.Windows;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class CoefficientAssessmentTab
    {
        private CoefficientAssessmentTabViewModel _viewmodel;
        
        public CoefficientAssessmentTab()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (CoefficientAssessmentTabViewModel) DataContext;
        }
    }
}