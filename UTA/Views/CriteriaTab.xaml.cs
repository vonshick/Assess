using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class CriteriaTab : UserControl
    {
        private int _lastDataGridSelectedItem = -1;
        private CriteriaTabViewModel _viewmodel;


        public CriteriaTab()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }


        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (CriteriaTabViewModel) DataContext;

            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(_viewmodel.NameTextBoxFocusTrigger))
                {
                    NameTextBox.Focus();
                }
                else if (args.PropertyName == nameof(_viewmodel.CriterionIndexToShow))
                {
                    CriteriaDataGrid.SelectedIndex = _viewmodel.CriterionIndexToShow;
                    if (CriteriaDataGrid.SelectedItem == null) return;
                    CriteriaDataGrid.ScrollIntoView(CriteriaDataGrid.SelectedItem);
                }
            };

            if (_viewmodel.CriterionIndexToShow != -1)
            {
                CriteriaDataGrid.SelectedIndex = _viewmodel.CriterionIndexToShow;
                if (CriteriaDataGrid.SelectedItem != null) CriteriaDataGrid.ScrollIntoView(CriteriaDataGrid.SelectedItem);
            }
            else
            {
                CriteriaDataGrid.SelectedItem = null;
            }

            NameTextBox.Focus();
        }

        private void CriteriaDataGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) e.Handled = true;
        }

        private void DataGridRowClicked(object sender, DataGridRowDetailsEventArgs e)
        {
            var criteriaList = (IList<Criterion>) ((DataGrid) sender).ItemsSource;
            var selectedCriterion = (Criterion) e.Row.Item;
            _lastDataGridSelectedItem = criteriaList.IndexOf(selectedCriterion);
        }

        private void TabUnloaded(object sender, RoutedEventArgs e)
        {
            if (_lastDataGridSelectedItem < _viewmodel.Criteria.CriteriaCollection.Count)
                _viewmodel.CriterionIndexToShow = _lastDataGridSelectedItem;
        }

        // focus when criterion is added
        private void NameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox) sender).Text == "") NameTextBox.Focus();
        }
    }
}