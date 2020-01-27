// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Assess.ViewModels;
using DataModel.Input;

namespace Assess.Views
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