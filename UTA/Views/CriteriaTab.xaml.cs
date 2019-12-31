using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataModel.Input;
using UTA.ViewHelperClasses;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class CriteriaTab : UserControl
    {
        private CriteriaTabViewModel _viewmodel;

        public CriteriaTab()
        {
            Loaded += ViewLoaded;
            CriteriaTypeList = Enum.GetNames(typeof(Criterion.CriterionDirectionTypes)).ToList();
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = ((MainViewModel)DataContext).CriteriaTabViewModel;
            CriteriaDirectionComboBoxColumn.ItemsSource = CriteriaTypeList;
            ButtonEditCriteria.Content = "Editing is OFF";
            EditCriteriaDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
            EditCriteriaDataGrid.Unloaded += DataGridUnloaded;
        }

        public List<string> CriteriaTypeList { get; set; }

        void DataGridUnloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (!grid.IsReadOnly)
                _viewmodel.RemovePlaceholder();
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private int errorCount;
        private void OnErrorEvent(object sender, RoutedEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null)
                throw new Exception("Unexpected event args");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                {
                    errorCount++; break;
                }
                case ValidationErrorEventAction.Removed:
                {
                    errorCount = 0; break;
                }
                default:
                {
                    throw new Exception("Unknown action");
                }
            }
            ButtonEditCriteria.IsEnabled = errorCount == 0;
        }

        private DataGridRow GetAlternativesDataGridRow(int index)
        {
            DataGridRow row =
                (DataGridRow)EditCriteriaDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                EditCriteriaDataGrid.UpdateLayout();
                EditCriteriaDataGrid.ScrollIntoView(EditCriteriaDataGrid.Items[index]);
                row = (DataGridRow)EditCriteriaDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return row;
        }

        private DataGridCell GetAlternativesDataGridCell(DataGridRow row, int index)
        {
            DataGridCellsPresenter presenter = VisualChildHelper.GetVisualChild<DataGridCellsPresenter>(row);
            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(index);
            if (cell == null)
            {
                //todo check if works
                //                EditAlternativesDataGrid.ScrollIntoView(rowContainer, EditAlternativesDataGrid.Columns[column]);
                EditCriteriaDataGrid.ScrollIntoView(row, EditCriteriaDataGrid.Columns[index]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return cell;
        }

        void NewRowCellFocusLost(object sender, EventArgs e)
        {
//            var cell = (DataGridCell)sender;
//            if (cell.IsEditing)
//            {
//                if (((TextBox)cell.Content).Text == "")
//                {
//                    if ((string)cell.Column.Header == "Name")
//                        _viewmodel.Alternatives
//                            .AlternativesCollection[_viewmodel.Alternatives.AlternativesCollection.Count - 1]
//                            .Name = "name";
//                    else
//                        _viewmodel.Alternatives
//                            .AlternativesCollection[_viewmodel.Alternatives.AlternativesCollection.Count - 1]
//                            .Description = "description";
//                }
//            }
        }

        void NewRowCellClicked(object sender, EventArgs e)
        {
            var cell = (DataGridCell)sender;
            if (cell.IsEditing)
            {
                var textBox = (TextBox)cell.Content;
                if (cell.IsEditing && textBox.Text == "name" || textBox.Text == "description")
                    textBox.Text = "";
            }
        }

        private void ButtonSaveCurrentPlaceholderClicked(object sender, RoutedEventArgs e)
        {
            _viewmodel.SaveCurrentPlaceholder();
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (EditCriteriaDataGrid.ItemContainerGenerator.Status
                == GeneratorStatus.ContainersGenerated && !EditCriteriaDataGrid.IsReadOnly)
            {
                //todo: error, still can return null! happened with lot of criteria and when clicked cell in criteria column
                DataGridRow row = GetAlternativesDataGridRow(EditCriteriaDataGrid.Items.Count - 1);
                //                  Setter italic = new Setter(TextBlock.FontStyleProperty, FontStyles.Italic, null);
                //                  Style newStyle = new Style(row.GetType());
                //                  newStyle.Setters.Add(italic);
                //                  row.Style = newStyle;
                EditCriteriaDataGrid.ItemContainerGenerator.StatusChanged -=
                    ItemContainerGeneratorStatusChanged;

                DataGridCell cell = GetAlternativesDataGridCell(row, 0);
                Button btn = new Button();
                btn.Click += ButtonSaveCurrentPlaceholderClicked;
                btn.Content = "Add";
                cell.Content = btn;

                cell = GetAlternativesDataGridCell(row, 1);
                cell.Foreground = Brushes.Gray;
                cell.GotFocus += NewRowCellClicked;
                cell.LostFocus += NewRowCellFocusLost;

                cell = GetAlternativesDataGridCell(row, 2);
                cell.Foreground = Brushes.Gray;
                cell.GotFocus += NewRowCellClicked;
                cell.LostFocus += NewRowCellFocusLost;

                //                  EditAlternativesDataGrid.UpdateLayout();
            }
        }

        private void EditCriteriaSwitchClicked(object sender, RoutedEventArgs e)
        {
            EditCriteriaDataGrid.IsReadOnly = !EditCriteriaDataGrid.IsReadOnly;
            if (EditCriteriaDataGrid.IsReadOnly)
            {
                ButtonEditCriteria.Content = "Editing is OFF";
                EditCriteriaDataGrid.Columns[0].Visibility = Visibility.Collapsed;
                EditCriteriaDataGrid.ItemContainerGenerator.StatusChanged -= ItemContainerGeneratorStatusChanged;
                _viewmodel.RemovePlaceholder();
            }
            else
            {
                ButtonEditCriteria.Content = "Editing is ON";
                EditCriteriaDataGrid.Columns[0].Visibility = Visibility.Visible;
                EditCriteriaDataGrid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
                _viewmodel.AddPlaceholder();
            }
        }
        private void RemoveCriteriaButtonClicked(object sender, RoutedEventArgs e)
        {
            if (EditCriteriaDataGrid.SelectedItem is Criterion criterion)
            {
                _viewmodel.Criteria.RemoveCriterion(criterion);
            }
        }

        //todo deprecated, remove window and related classes/methods
        public void ShowAddCriterionDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            _viewmodel.ShowAddCriterionDialog();
        }
    }
}