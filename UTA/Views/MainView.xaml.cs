using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            // DataLoader csvLoader = SampleImport.ProcessSampleData(true, false, false, false); // csv
            // DataLoader utxLoader = SampleImport.ProcessSampleData(false, true, false, false); // utx
            // DataLoader xmlLoader = SampleImport.ProcessSampleData(false, false, true, false); // xml
            

            //TODO vonshick REMOVE IT AFTER TESTING
            // XMCDALoader xmcdaLoader = SampleImport.ProcessXMCDA(); // xmcda            
            // SampleExport.exportXMCDA(xmcdaLoader.CriterionList, xmcdaLoader.AlternativeList);

            InitializeComponent();
            SetBindings();
            InitComboBoxCriterionType();
            _viewmodel.PropertyChanged += ViewmodelPropertyChanged;
            _viewmodel.GenerateCriteriaTable();
            CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
            _viewmodel.GenerateAlternativesTable();
        }

        private void InitComboBoxCriterionType()
        {
            CriterionDirectionTypes = MainViewModel.CriterionDirectionTypes;
            ComboBoxCriterionType.SelectedValuePath = "Key";
            ComboBoxCriterionType.DisplayMemberPath = "Value";
            foreach (KeyValuePair<string, string> criterionDirectionType in CriterionDirectionTypes)
            {
                ComboBoxCriterionType.Items.Add(criterionDirectionType);
            }
            ComboBoxCriterionType.SelectedIndex = 0;
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeDescription") { Source = this });
            AlternativesListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            TextBoxCriterionName.SetBinding(TextBox.TextProperty, new Binding("InputCriterionName") { Source = this });
            TextBoxCriterionDescription.SetBinding(TextBox.TextProperty, new Binding("InputCriterionDescription") { Source = this });
            ComboBoxCriterionType.SetBinding(ComboBox.SelectedValueProperty, new Binding("InputCriterionDirection") { Source = this });
            TextBoxCriterionLinear.SetBinding(TextBox.TextProperty, new Binding("InputCriterionLinearSegments") { Source = this });
            CriteriaListView.SetBinding(ListView.ItemsSourceProperty, new Binding("CriteriaTable") { Source = _viewmodel });
        }

        public string InputAlternativeName { get; set; }
        public string InputAlternativeDescription { get; set; }
        public string InputCriterionDescription { get; set; }
        public string InputCriterionName { get; set; }
        public string InputCriterionDirection { get; set; }
        public string InputCriterionLinearSegments { get; set; }
        public static Dictionary<string, string> CriterionDirectionTypes { get; set; }

        public void ViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AlternativesTable":
                    AlternativesListView.View = GenerateGridView(_viewmodel.AlternativesTable);
                    break;
                case "CriteriaTable":
//                    CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
                    break;
                default:
                    throw new Exception("error in prop: " + e.PropertyName);
            }
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

        private void AddCriterion(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(InputCriterionLinearSegments, out var linearSegments))
            {
                if (inputNotEmpty(InputCriterionName, InputCriterionDescription, InputCriterionDirection))
                {
                    _viewmodel.AddCriterion(InputCriterionName, InputCriterionDescription, InputCriterionDirection, linearSegments);
                    TextBoxCriterionName.Clear();
                    TextBoxCriterionDescription.Clear();
                    InputCriterionName = "";
                    InputCriterionDescription = "";
                }
                else
                {
                    //todo notify user
                    Console.WriteLine("No field can be empty!");
                }
            }
            else
            {
                //todo notify user
                //limit? short int or less?
                Console.WriteLine("Linear segments must be int!");
            }

        }

        private void AddAlternative(object sender, RoutedEventArgs e)
        {
            if (inputNotEmpty(InputAlternativeName, InputAlternativeDescription))
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

        private bool inputNotEmpty(params string[] inputs)
        {
            if (inputs.Length == 0)
            {
                return false;
            }
            foreach (string input in inputs)
            {
                if (input is null)
                {
                    return false;
                }
            }
            return true;
        }
    }

}
