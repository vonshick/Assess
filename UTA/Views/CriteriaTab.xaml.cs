using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataModel.Input;
using UTA.OtherViewClasses;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class CriteriaTab : UserControl
    {
        private int _errorCount;
        private CriteriaTabViewModel _viewmodel;

        public CriteriaTab()
        {
            Loaded += ViewLoaded;
            CriteriaTypeList = Criterion.CriterionDirectionTypesList;
            InitializeComponent();
        }

        public List<string> CriteriaTypeList { get; set; }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = ((MainViewModel) DataContext).CriteriaTabViewModel;
            CriteriaDirectionComboBoxColumn.ItemsSource = CriteriaTypeList;
            AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
        }

        //todo could find better way for it if enough time
        private void OnErrorEvent(object sender, RoutedEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null)
                throw new Exception("Unexpected event args");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                {
                    _errorCount++;
                    break;
                }
                case ValidationErrorEventAction.Removed:
                {
                    _errorCount = 0;
                    break;
                }
                default:
                {
                    throw new Exception("Unknown action");
                }
            }

            ButtonEditCriteria.IsEnabled = _errorCount == 0;
        }

        private DataGridRow GetCriteriaDataGridRow(int index)
        {
            var row =
                (DataGridRow) EditCriteriaDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                EditCriteriaDataGrid.UpdateLayout();
                EditCriteriaDataGrid.ScrollIntoView(EditCriteriaDataGrid.Items[index]);
                row = (DataGridRow) EditCriteriaDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            if (row == null)
                Console.WriteLine("Row null index " + index);
            return row;
        }

        private DataGridCell GetCriteriaDataGridCell(DataGridRow row, int index)
        {
            var presenter = VisualChildHelper.GetVisualChild<DataGridCellsPresenter>(row);
            var cell = (DataGridCell) presenter.ItemContainerGenerator.ContainerFromIndex(index);
            if (cell == null)
            {
                //todo check if works
                //                EditAlternativesDataGrid.ScrollIntoView(rowContainer, EditAlternativesDataGrid.Columns[column]);
                EditCriteriaDataGrid.UpdateLayout();
                EditCriteriaDataGrid.ScrollIntoView(row, EditCriteriaDataGrid.Columns[index]);
                cell = (DataGridCell) presenter.ItemContainerGenerator.ContainerFromIndex(index);
            }

            if (cell == null)
                Console.WriteLine("Cell null index " + index);
            return cell;
        }

        private void NewRowCellFocusLost(object sender, EventArgs e)
        {
            var cell = (DataGridCell) sender;
            if (cell.IsEditing)
                if (((TextBox) cell.Content).Text == "")
                {
                    if ((string) cell.Column.Header == "Name")
                        _viewmodel.Criteria
                            .CriteriaCollection[_viewmodel.Criteria.CriteriaCollection.Count - 1]
                            .Name = "name";
                    else
                        _viewmodel.Criteria
                            .CriteriaCollection[_viewmodel.Criteria.CriteriaCollection.Count - 1]
                            .Description = "description";
                }
        }

        private void NewRowCellClicked(object sender, EventArgs e)
        {
            var cell = (DataGridCell) sender;
            if (cell.IsEditing)
            {
                var textBox = (TextBox) cell.Content;
                if (cell.IsEditing && textBox.Text == "name" || textBox.Text == "description")
                    textBox.Text = "";
            }
        }

        private void ButtonSaveCurrentPlaceholderClicked(object sender, RoutedEventArgs e)
        {
            if (_errorCount == 0)
            {
                EditCriteriaDataGrid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
                _viewmodel.SaveCurrentPlaceholder();
            }
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (EditCriteriaDataGrid.ItemContainerGenerator.Status
                == GeneratorStatus.ContainersGenerated && !EditCriteriaDataGrid.IsReadOnly)
            {
                //todo: error, still can return null! happened with lot of criteria and when clicked cell in criteria column
                Console.WriteLine("Row idx (grid.items.count): " + (EditCriteriaDataGrid.Items.Count - 1) +
                                  ", gird.itemcontainergen.count: " + EditCriteriaDataGrid.ItemContainerGenerator.Items.Count);
                var row = GetCriteriaDataGridRow(EditCriteriaDataGrid.Items.Count - 1);
                //                  Setter italic = new Setter(TextBlock.FontStyleProperty, FontStyles.Italic, null);
                //                  Style newStyle = new Style(row.GetType());
                //                  newStyle.Setters.Add(italic);
                //                  row.Style = newStyle;
                EditCriteriaDataGrid.ItemContainerGenerator.StatusChanged -=
                    ItemContainerGeneratorStatusChanged;

                var cell = GetCriteriaDataGridCell(row, 0);
                var btn = new Button();
                btn.Click += ButtonSaveCurrentPlaceholderClicked;
                btn.Content = "Add";
                cell.Content = btn;

                cell = GetCriteriaDataGridCell(row, 1);
                cell.Foreground = Brushes.Gray;
                cell.GotFocus += NewRowCellClicked;
                cell.LostFocus += NewRowCellFocusLost;

                cell = GetCriteriaDataGridCell(row, 2);
                cell.Foreground = Brushes.Gray;
                cell.GotFocus += NewRowCellClicked;
                cell.LostFocus += NewRowCellFocusLost;
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

        private void RemoveCriteriaButtonClicked(string name)
        {
            Console.WriteLine("Remove btn clicked by " + name);
            if (EditCriteriaDataGrid.SelectedItem is Criterion criterion) _viewmodel.Criteria.RemoveCriterion(criterion);
        }

        private void RemoveCriteriaButtonClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Remove btn clicked"); //by who?

            if (EditCriteriaDataGrid.SelectedItem is Criterion criterion) _viewmodel.Criteria.RemoveCriterion(criterion);
        }
    }
}