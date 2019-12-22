using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UTA.Models;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class AddCriterionView : Window
    {
        public AddCriterionViewModel AddCriterionViewModel { get; set; }

        public AddCriterionView(AddCriterionViewModel addCriterionViewModel)
        {
            AddCriterionViewModel = addCriterionViewModel;
            InitializeComponent();
            SetBindings();
        }

        private void SetBindings()
        {
            TextBoxCriterionName.SetBinding(TextBox.TextProperty, new Binding("InputCriterionName") { Source = this });
            TextBoxCriterionDescription.SetBinding(TextBox.TextProperty, new Binding("InputCriterionDescription") { Source = this });
            ComboBoxCriterionType.SetBinding(ComboBox.SelectedValueProperty, new Binding("InputCriterionDirection") { Source = this });
            ComboBoxCriterionType.SetBinding(ComboBox.ItemsSourceProperty, new Binding("CriteriaTypeList") { Source = AddCriterionViewModel });
            TextBoxCriterionLinear.SetBinding(TextBox.TextProperty, new Binding("InputCriterionLinearSegments") { Source = this });
        }

        public string InputCriterionDescription { get; set; }
        public string InputCriterionName { get; set; }
        public string InputCriterionDirection { get; set; }
        public string InputCriterionLinearSegments { get; set; }
        public bool AddedCriterion { get; set; } = false;


        private void AddCriterion(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(InputCriterionLinearSegments, out var linearSegments))
            {
                if (DataValidation.StringsNotEmpty(InputCriterionName, InputCriterionDescription, InputCriterionDirection))
                {
                    if (AddCriterionViewModel.AddCriterion(InputCriterionName, InputCriterionDescription, InputCriterionDirection,
                        linearSegments))
                    {
                        ButtonAddCriterion.Content = "Added";
                        AddedCriterion = true;
                        this.Close();
                    }
                    else
                    {
                        ButtonAddCriterion.Content = "Failed";
                    }
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
    }
}
