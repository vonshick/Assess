using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using DataModel.Input;
using UTA.Models.DataValidation;
using UTA.ViewHelperClasses;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class AlternativesTab : UserControl
    {

        private AlternativesTabViewModel _viewmodel;

        public AlternativesTab()
        {
            Console.WriteLine("AltTab init");
            Loaded += ViewLoaded;
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = ((MainViewModel) DataContext).AlternativesTabViewModel;
            SetBindings();
            EditAlternativesDataGrid.AddingNewItem += _viewmodel.AddAlternativeFromDataGrid;
            AddAlternativesDataGridCriteriaColumns();
            ButtonEditAlternatives.Content = "Editing is OFF";
            EditAlternativesDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
            EditAlternativesDataGrid.Unloaded += DataGridUnloaded;
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty,
                new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty,
                new Binding("InputAlternativeDescription") { Source = this });
        }

        public string InputAlternativeName { get; set; }
        public string InputAlternativeDescription { get; set; }

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
                    errorCount=0; break;
                }
                default:
                {
                    throw new Exception("Unknown action");
                }
            }
            ButtonEditAlternatives.IsEnabled = errorCount == 0;
        }

        private void AddAlternativesDataGridCriteriaColumns()
        {
            int index = 0;
            //adding criteria columns
            foreach (var criterion in _viewmodel.Criteria.CriteriaCollection)
            {
                if(criterion != _viewmodel.Criteria.Placeholder) 
                    AddAlternativesDataGridColumn(criterion, index++);
            }
        }

        private void AddAlternative(object sender, RoutedEventArgs e)
        {
            if (DataValidation.StringsNotEmpty(InputAlternativeName, InputAlternativeDescription))
            {
                _viewmodel.AddAlternative(InputAlternativeName, InputAlternativeDescription);
                //todo uncomment after tests
//                TextBoxAlternativeName.Clear();
//                TextBoxAlternativeDescription.Clear();
//                InputAlternativeName = "";
//                InputAlternativeDescription = "";
            }
            else
            {
                //todo notify user
            }
        }

        private void RemoveAlternativeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (EditAlternativesDataGrid.SelectedItem is Alternative alternative)
            {
                _viewmodel.Alternatives.RemoveAlternative(alternative);
            }
        }

        private void EditAlternativesSwitchClicked(object sender, RoutedEventArgs e)
        {
            EditAlternativesDataGrid.IsReadOnly = !EditAlternativesDataGrid.IsReadOnly;
            if (EditAlternativesDataGrid.IsReadOnly)
            {
                ButtonEditAlternatives.Content = "Editing is OFF";
                EditAlternativesDataGrid.Columns[0].Visibility = Visibility.Collapsed;
                EditAlternativesDataGrid.ItemContainerGenerator.StatusChanged -= ItemContainerGeneratorStatusChanged;
                _viewmodel.RemovePlaceholder();
            }
            else
            {
                ButtonEditAlternatives.Content = "Editing is ON";
                EditAlternativesDataGrid.Columns[0].Visibility = Visibility.Visible;
                EditAlternativesDataGrid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
                _viewmodel.AddPlaceholder();
            }
        }

        void NewRowCellFocusLost(object sender, EventArgs e)
        {
            var cell = (DataGridCell) sender;
            if (cell.IsEditing)
            {
                if (((TextBox) cell.Content).Text == "")
                {
                    if ((string) cell.Column.Header == "Name")
                        _viewmodel.Alternatives
                            .AlternativesCollection[_viewmodel.Alternatives.AlternativesCollection.Count - 1]
                            .Name = "name";
                    else
                        _viewmodel.Alternatives
                            .AlternativesCollection[_viewmodel.Alternatives.AlternativesCollection.Count - 1]
                            .Description = "description";
                }
            }
        }

        void NewRowCellClicked(object sender, EventArgs e)
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
            Console.WriteLine(errorCount);
            if (errorCount == 0)
            {
                EditAlternativesDataGrid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
                _viewmodel.SaveCurrentPlaceholder();
            }
        }

        public bool IsValid(DependencyObject parent)
        {
            if (Validation.GetHasError(parent))
                return false;

            // Validate all the bindings on the children
            for (int i = 0; i != VisualTreeHelper.GetChildrenCount(parent); ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (!IsValid(child)) { return false; }
            }

            return true;
        }

        private DataGridRow GetAlternativesDataGridRow(int index)
        {
            DataGridRow row =
                (DataGridRow)EditAlternativesDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                EditAlternativesDataGrid.UpdateLayout();
                EditAlternativesDataGrid.ScrollIntoView(EditAlternativesDataGrid.Items[index]);
                row = (DataGridRow)EditAlternativesDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
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
                EditAlternativesDataGrid.ScrollIntoView(row, EditAlternativesDataGrid.Columns[index]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return cell;
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (EditAlternativesDataGrid.ItemContainerGenerator.Status
                == GeneratorStatus.ContainersGenerated && !EditAlternativesDataGrid.IsReadOnly)
            {
                //todo: error, still can return null! happened with lot of criteria and when clicked cell in criteria column
                DataGridRow row = GetAlternativesDataGridRow(EditAlternativesDataGrid.Items.Count - 1);
                //                  Setter italic = new Setter(TextBlock.FontStyleProperty, FontStyles.Italic, null);
                //                  Style newStyle = new Style(row.GetType());
                //                  newStyle.Setters.Add(italic);
                //                  row.Style = newStyle;
                EditAlternativesDataGrid.ItemContainerGenerator.StatusChanged -=
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

        private void AddAlternativesDataGridColumn(Criterion criterion, int startingIndex)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn
            {
                Binding = new Binding()
                {
                    Path = new PropertyPath("CriteriaValuesList[" + startingIndex + "].Value"),
                    Mode = BindingMode.TwoWay
                }
            };
            EditAlternativesDataGrid.Columns.Add(textColumn);

            BindingProxy bindingProxy = new BindingProxy {Data = criterion};

            string critResName = "criterion[" + startingIndex + "]";
            EditAlternativesDataGrid.Resources.Add(critResName, bindingProxy);
            //            textColumn.DataContext = (Criterion)header.Resources["criterion"];
            //            textColumn.Header = new Binding() { Path = new PropertyPath("Name"), Source = (Criterion)EditAlternativesDataGrid.Resources[critResName] };
            TextBlock header = new TextBlock();
            header.SetBinding(TextBlock.TextProperty,
                new Binding()
                {
                    Path = new PropertyPath("Data.Name"),
                    Source = (BindingProxy) EditAlternativesDataGrid.Resources[critResName]
                });
            textColumn.Header = header;
        }
    }
}