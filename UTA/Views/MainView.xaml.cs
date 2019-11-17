using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UTA.ViewModels;

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
            InitializeComponent();
            CriteriaTypeList = _viewmodel.CriteriaTypeList;
            SetBindings();
            _viewmodel.PropertyChanged += ViewmodelPropertyChanged;
            _viewmodel.GenerateCriteriaTable();
            CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
            _viewmodel.GenerateAlternativesTable();
            ComboBoxCriterionType.SelectedIndex = 0;
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeDescription") { Source = this });
            AlternativesListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            TextBoxCriterionName.SetBinding(TextBox.TextProperty, new Binding("InputCriterionName") { Source = this });
            TextBoxCriterionDescription.SetBinding(TextBox.TextProperty, new Binding("InputCriterionDescription") { Source = this });
            ComboBoxCriterionType.SetBinding(ComboBox.ItemsSourceProperty, new Binding("CriteriaTypeList") { Source = this });
            ComboBoxCriterionType.SetBinding(ComboBox.SelectedItemProperty, new Binding("InputCriterionTypeString") { Source = this });
            TextBoxCriterionLinear.SetBinding(TextBox.TextProperty, new Binding("InputCriterionLinearSegments") { Source = this });
            CriteriaListView.SetBinding(ListView.ItemsSourceProperty, new Binding("CriteriaTable") { Source = _viewmodel });
        }

        public string InputAlternativeName { get; set; }
        public string InputAlternativeDescription { get; set; }
        public string InputCriterionDescription { get; set; }
        public string InputCriterionName { get; set; }
        public string InputCriterionTypeString { get; set; }
        public int InputCriterionLinearSegments { get; set; }
        public List<string> CriteriaTypeList { get; set; }

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
            _viewmodel.AddCriterion(InputCriterionTypeString, InputCriterionName, InputCriterionDescription, InputCriterionLinearSegments);
            TextBoxCriterionName.Clear();
            TextBoxCriterionDescription.Clear();
            TextBoxCriterionLinear.Clear();
        }

        private void AddAlternative(object sender, RoutedEventArgs e)
        {
            _viewmodel.AddAlternative(InputAlternativeName, InputAlternativeDescription);
            TextBoxAlternativeName.Clear();
            TextBoxAlternativeDescription.Clear();
        }
    }

}
