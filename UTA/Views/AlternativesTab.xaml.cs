using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class AlternativesTab : UserControl
    {
        private readonly Style _criterionValueCellStyle;
        private readonly Style _criterionValueHeaderStyle;
        private readonly List<DataGridTextColumn> _criterionValuesColumns;
        private int _lastDataGridSelectedItem = -1;
        private AlternativesTabViewModel _viewmodel;


        public AlternativesTab()
        {
            InitializeComponent();
            _criterionValuesColumns = new List<DataGridTextColumn>();
            _criterionValueHeaderStyle = (Style) FindResource("CenteredDataGridColumnHeader");
            _criterionValueCellStyle = (Style) FindResource("CenteredDataGridCell");

            Loaded += ViewLoaded;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (AlternativesTabViewModel) DataContext;

            _viewmodel.Criteria.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName != nameof(_viewmodel.Criteria.CriteriaCollection)) return;
                InitializeCriterionValueColumnsGenerator();
                GenerateCriterionValuesColumns();
            };
            InitializeCriterionValueColumnsGenerator();

            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(_viewmodel.NameTextBoxFocusTrigger))
                {
                    NameTextBox.Focus();
                }
                else if (args.PropertyName == nameof(_viewmodel.AlternativeIndexToShow))
                {
                    AlternativesDataGrid.SelectedIndex = _viewmodel.AlternativeIndexToShow;
                    if (AlternativesDataGrid.SelectedItem == null) return;
                    AlternativesDataGrid.ScrollIntoView(AlternativesDataGrid.SelectedItem);
                }
            };

            if (_viewmodel.AlternativeIndexToShow != -1)
            {
                AlternativesDataGrid.SelectedIndex = _viewmodel.AlternativeIndexToShow;
                if (AlternativesDataGrid.SelectedItem != null) AlternativesDataGrid.ScrollIntoView(AlternativesDataGrid.SelectedItem);
            }
            else
            {
                AlternativesDataGrid.SelectedItem = null;
            }

            NameTextBox.Focus();
            GenerateCriterionValuesColumns();
        }

        private void InitializeCriterionValueColumnsGenerator()
        {
            _viewmodel.Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                    GenerateCriterionValuesColumns();
            };
        }

        private void GenerateCriterionValuesColumns()
        {
            foreach (var criterionValuesColumn in _criterionValuesColumns) AlternativesDataGrid.Columns.Remove(criterionValuesColumn);
            _criterionValuesColumns.Clear();

            for (var i = 0; i < _viewmodel.Criteria.CriteriaCollection.Count; i++)
            {
                var criterionValueColumn = new DataGridTextColumn
                {
                    Binding = new Binding
                    {
                        Path = new PropertyPath($"CriteriaValuesList[{i}].Value"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    Header = _viewmodel.Criteria.CriteriaCollection[i].Name,
                    HeaderStyle = _criterionValueHeaderStyle,
                    CellStyle = _criterionValueCellStyle,
                    MinWidth = 113
                };

                _criterionValuesColumns.Add(criterionValueColumn);
                AlternativesDataGrid.Columns.Add(criterionValueColumn);
            }
        }

        private void CriteriaDataGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) e.Handled = true;
        }

        private void DataGridRowClicked(object sender, DataGridRowDetailsEventArgs e)
        {
            var alternativesList = (IList<Alternative>) ((DataGrid) sender).ItemsSource;
            var selectedAlternative = (Alternative) e.Row.Item;
            _lastDataGridSelectedItem = alternativesList.IndexOf(selectedAlternative);
        }

        private void TabUnloaded(object sender, RoutedEventArgs e)
        {
            if (_lastDataGridSelectedItem < _viewmodel.Alternatives.AlternativesCollection.Count)
                _viewmodel.AlternativeIndexToShow = _lastDataGridSelectedItem;
        }

        // focus when alternative is added
        private void NameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox) sender).Text == "") NameTextBox.Focus();
        }
    }
}