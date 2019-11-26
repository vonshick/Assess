using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
//            InitComboBoxCriterionType();
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
        private void InitComboBoxCriterionType()
        {
            CriteriaTypeList = AddCriterionViewModel.CriteriaTypeList;
            /*CriterionDirectionTypes = MainViewModel.CriterionDirectionTypes;
            ComboBoxCriterionType.SelectedValuePath = "Key";
            ComboBoxCriterionType.DisplayMemberPath = "Value";
            foreach (KeyValuePair<string, string> criterionDirectionType in CriterionDirectionTypes)
            {
                ComboBoxCriterionType.Items.Add(criterionDirectionType);
            }
            ComboBoxCriterionType.SelectedIndex = 0;*/
        }

        public MainViewModel MainViewModel { get; set; }
        public string InputCriterionDescription { get; set; }
        public string InputCriterionName { get; set; }
        public string InputCriterionDirection { get; set; }
        public string InputCriterionLinearSegments { get; set; }
        //        public static Dictionary<string, string> CriterionDirectionTypes { get; set; }
        public List<string> CriteriaTypeList { get; set; }
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
