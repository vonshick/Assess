using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataModel.Input;
using UTA.Models;
using UTA.ViewModels;
// using ImportModule; // TODO vonshick REMOVE IT AFTER TESTING
// using ExportModule; // TODO vonshick REMOVE IT AFTER TESTING
namespace UTA.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewmodel = new MainViewModel();

        public MainView()
        {
            //TODO vonshick REMOVE IT AFTER TESTING
            // DataLoader csvLoader = SampleImport.ProcessSampleData(true, false, false); // csv
            // DataLoader utxLoader = SampleImport.ProcessSampleData(false, true, false); // utx
            // DataLoader xmlLoader = SampleImport.ProcessSampleData(false, false, true); // xml
//            TODO vonshick REMOVE IT AFTER TESTING
//             XMCDALoader xmcdaLoader = SampleImport.ProcessXMCDA(); // xmcda            
//             SampleExport.exportXMCDA(xmcdaLoader.CriterionList, xmcdaLoader.AlternativeList);

            InitializeComponent();
            SetBindings();
            _viewmodel.PropertyChanged += ViewmodelPropertyChanged;
            _viewmodel.Criteria.CriteriaCollection.CollectionChanged += UpdateAlternativesDataGridColumns;
            _viewmodel.GenerateCriteriaTable();
            _viewmodel.GenerateAlternativesTable();
            ButtonEditAlternatives.Content = "Editing is OFF";
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeDescription") { Source = this });

            EditAlternativesDataGrid.ItemsSource = _viewmodel.Alternatives.AlternativesCollection;
            //            EditAlternativesDataGrid.CellEditEnding += RowEditEnding;
            EditAlternativesDataGrid.AddingNewItem += _viewmodel.AddAlternativeFromDataGrid;

            AlternativesListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            CriteriaListView.SetBinding(ListView.ItemsSourceProperty, new Binding("CriteriaTable") { Source = _viewmodel });
            
            RankingListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            RankingListView2.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
        }

        public string InputAlternativeName { get; set; }
        public string InputAlternativeDescription { get; set; }

        public void ViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AlternativesTable":
                    RenderListViews();
                    break;
                case "CriteriaTable":
//                    CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
                    break;
                default:
                    throw new Exception("error in prop: " + e.PropertyName);
            }
        }
        private void RenderListViews()
        {
            AlternativesListView.View = GenerateGridView(_viewmodel.AlternativesTable);
            CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
            RankingListView.View = GenerateGridView(_viewmodel.AlternativesTable);
            RankingListView2.View = GenerateGridView(_viewmodel.AlternativesTable);
        }

        private GridView GenerateGridView(DataTable table)
        {
            GridView view = new GridView();
            foreach (DataColumn column in table.Columns)
            {
                view.Columns.Add(new GridViewColumn()
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName)
                });
            }
            return view;
        }

        public void ShowAddCriterionDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            _viewmodel.ShowAddCriterionDialog();
        }

        private void AddAlternative(object sender, RoutedEventArgs e)
        {
            if (DataValidation.StringsNotEmpty(InputAlternativeName, InputAlternativeDescription))
            {
                _viewmodel.AddAlternative(InputAlternativeName, InputAlternativeDescription);
                TextBoxAlternativeName.Clear();
                TextBoxAlternativeDescription.Clear();
                InputAlternativeName = "";
                InputAlternativeDescription = "";
            }
            else
            {
                //todo notify user
            }
        }

        private void EditAlternativesSwitch(object sender, RoutedEventArgs e)
        {
            EditAlternativesDataGrid.IsReadOnly = !EditAlternativesDataGrid.IsReadOnly;
            if (EditAlternativesDataGrid.IsReadOnly)
            {
//                _viewmodel.UpdateAlternatives();
                ButtonEditAlternatives.Content = "Editing is OFF";
            }
            else
            {
                ButtonEditAlternatives.Content = "Editing is ON";
            }
        }

        public void UpdateAlternativesDataGridColumns(object sender, NotifyCollectionChangedEventArgs e)
        {
            int startingIndex = e.NewStartingIndex;
            foreach (Criterion criterion in e.NewItems)
            {
                AddAlternativesDataGridColumn(criterion, startingIndex++);
            }
        }

        private void AddAlternativesDataGridColumn(Criterion criterion, int startingIndex)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn {Header = criterion.Name, Binding = new Binding() { Mode = BindingMode.TwoWay, Path = new PropertyPath("CriteriaValuesList[" + startingIndex + "].Value") } };
            EditAlternativesDataGrid.Columns.Add(textColumn);
        }

        /*private void RowEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //event fired right before edit end so no new values are present
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    Console.WriteLine("Edited row " + e.Row.GetIndex() + " col " + bindingPath);

//                    _viewmodel.GenerateAlternativesTable();

                    if (bindingPath == "Col2")
                    {
                        int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;
                        // rowIndex has the row index
                        // bindingPath has the column's binding
                        // el.Text has the new, user-entered value
                    }
                }
            }
        }*/

    }

}
