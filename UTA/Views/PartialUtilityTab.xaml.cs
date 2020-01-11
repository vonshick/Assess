using System;
using System.Windows;
using System.Windows.Controls;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class PartialUtilityTab : UserControl
    {
        private int _method;
        private PartialUtilityTabViewModel _viewmodel;

        public PartialUtilityTab()
        {
            Loaded += ViewLoaded;
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (PartialUtilityTabViewModel) DataContext;
        }

        private void StartDialogueButton_OnClick(object sender, RoutedEventArgs e)
        {
            var criterion = _viewmodel.Criterion;
            _method = int.Parse(Method.Text);
            //todo remove
            criterion.MinValue = float.Parse(StartPoint.Text);
            criterion.MaxValue = float.Parse(EndPoint.Text);
            Console.WriteLine(criterion.CriterionDirection);
            var dialogController = new DialogController(criterion, _method, 0.3f);
            var dialog = dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);
            ShowDialogueDialog(dialog, criterion);
        }

        private void ShowDialogueDialog(Dialog dialog, Criterion criterion)
        {
            StartButton.IsEnabled = false;

            var userDialogueDialogViewModel = new UserDialogueDialogViewModel(dialog, criterion, _method);
            var userDialogueDialog = new UserDialogueDialog();
            userDialogueDialog.DataContext = userDialogueDialogViewModel;
            if (_method == 3)
            {
                userDialogueDialog.ButtonSure.Content = "Lottery 1";
                userDialogueDialog.ButtonLottery.Content = "Lottery 2";
            }

            userDialogueDialog.ShowDialog();
            StartButton.IsEnabled = true;
            DigestResults(dialog);
        }

        private void DigestResults(Dialog dialog)
        {
            ResultsTextBlock.Text = "";

            if (_method != 3) ResultsTextBlock.Text = "wsp. równoważności = " + dialog.DisplayObject.X + ", ";
            ResultsTextBlock.Text += "punkty:\n";
            foreach (var point in dialog.DisplayObject.PointsList) ResultsTextBlock.Text += "(" + point.X + ";" + point.U + ")\n";
        }
    }
}