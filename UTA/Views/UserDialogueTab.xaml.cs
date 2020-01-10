using System;
using System.Windows;
using System.Windows.Controls;
using CalculationsEngine.Assess.Assess;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class UserDialogueTab : UserControl
    {
        private UserDialogueTabViewModel _viewmodel;
        private Dialog _dialog;
        private int _method;

        public UserDialogueTab()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (UserDialogueTabViewModel) DataContext;
        }

        private void StartDialogueButton_OnClick(object sender, RoutedEventArgs e)
        {
            //            CoefficientsDialog coefficientsDialog = new CoefficientsDialog(sampleCriteriaList);
            //            coefficientsDialog.GetCoefficientsForCriteria();

            // countKCoefficient(assessCoefficientsDialog.CriteriaCoefficientsList);

            DataModel.Input.Criterion criterion = _viewmodel.Criterion;
            string dir = "g";
            if (criterion.CriterionDirection == "Cost") dir = "c";
            Criterion assessCriterion = new Criterion(criterion.Name, dir, float.Parse(StartPoint.Text), float.Parse(EndPoint.Text));
            Console.WriteLine("Start dialogue for crit " + assessCriterion.Name + ", dir: " + assessCriterion.CriterionDirection + ", points: " + assessCriterion.MinValue + ", " + assessCriterion.MaxValue);
//            Assess.Criterion assessCriterion = new Criterion(criterion.Name, dir, criterion.MinValue, criterion.MaxValue);
            _method = int.Parse(Method.Text);
            DialogController dialogController = new DialogController(assessCriterion, _method, 0.3f);

            _dialog = (ConstantProbability) dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);
            _dialog.displayDialog();
            StartButton.IsEnabled = false;
            ChoiceButton.IsEnabled = true;
        }

        private void MakeChoiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            string choice = Choice.Text;
            if (choice == "1" || choice == "2" || choice == "3")
            {
                WarningLabel.Content = "";
                _dialog.ProcessDialog(choice);
                if (choice == "3")
                {
                    StartButton.IsEnabled = true;
                    ChoiceButton.IsEnabled = false;
                }
                else
                {
                    _dialog.displayDialog();
                }
            }
            else
                WarningLabel.Content = "Wrong choice!";
        }
    }
}