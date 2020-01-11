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
        private PartialUtilityTabViewModel _viewmodel;
        private int _method;

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
            Criterion criterion = _viewmodel.Criterion;
            _method = int.Parse(Method.Text);
            //todo remove
            criterion.MinValue = float.Parse(StartPoint.Text);
            criterion.MaxValue = float.Parse(EndPoint.Text);
            Console.WriteLine(criterion.CriterionDirection);
            DialogController dialogController = new DialogController(criterion, _method, 0.3f);
            var dialog = dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);
            ShowDialogueDialog(dialog, criterion);
        }

        private void ShowDialogueDialog(Dialog dialog, Criterion criterion)
        {
            StartButton.IsEnabled = false;

            UserDialogueDialogViewModel userDialogueDialogViewModel = new UserDialogueDialogViewModel(dialog, criterion, _method);
            UserDialogueDialog userDialogueDialog = new UserDialogueDialog();
            userDialogueDialog.DataContext = userDialogueDialogViewModel;
            if (_method == 3)
            {
                userDialogueDialog.ButtonSure.Content = "Lottery 1";
                userDialogueDialog.ButtonLottery.Content = "Lottery 2";
            }
            userDialogueDialog.ShowDialog();
            StartButton.IsEnabled = true;
        }


    }
}