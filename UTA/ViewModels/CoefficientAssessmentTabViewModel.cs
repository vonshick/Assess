using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalculationsEngine.Assess.Assess;
using DataModel.Input;
using DataModel.Results;
using UTA.Annotations;
using UTA.Helpers;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class CoefficientAssessmentTabViewModel : Tab, INotifyPropertyChanged
    {
        private readonly List<Criterion> _criteriaCollection;
        private readonly CoefficientsDialog _dialog;
        private int _currentCriterionIndex;
        private string _textOptionLottery;
        private string _textOptionSure;
        private readonly Action<List<CriterionCoefficient>> _showPartialUtilityTabs;

        public CoefficientAssessmentTabViewModel(List<Criterion> criteriaCollection, Action<List<CriterionCoefficient>> showPartialUtilityTabs)
        {
            Name = "Dialogue - Scaling Coefficient";
            _criteriaCollection = criteriaCollection;
            _showPartialUtilityTabs = showPartialUtilityTabs;
            _dialog = new CoefficientsDialog(criteriaCollection);
            _dialog.SetInitialValues(_criteriaCollection[_currentCriterionIndex = 0]);

            SetCoefficientsTextBlocks(_dialog);

            TakeSureCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(1);
                SetCoefficientsTextBlocks(_dialog);
            });
            TakeLotteryCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(2);
                SetCoefficientsTextBlocks(_dialog);
            });
            TakeIndifferentCommand = new RelayCommand(_ =>
            {
                _dialog.ProcessDialog(3);
                if (_currentCriterionIndex < _criteriaCollection.Count - 1)
                {
                    _dialog.SetInitialValues(_criteriaCollection[++_currentCriterionIndex]);
                    OnPropertyChanged(nameof(CurrentCriterion));
                    SetCoefficientsTextBlocks(_dialog);
                }
                else
                {
                    _showPartialUtilityTabs(_dialog.CriteriaCoefficientsList);
                }
            });
        }

        public Criterion CurrentCriterion => _criteriaCollection[_currentCriterionIndex];

        public RelayCommand TakeSureCommand { get; }
        public RelayCommand TakeLotteryCommand { get; }
        public RelayCommand TakeIndifferentCommand { get; }

        public string TextOptionSure
        {
            get => _textOptionSure;
            set
            {
                _textOptionSure = value;
                OnPropertyChanged(nameof(TextOptionSure));
            }
        }

        public string TextOptionLottery
        {
            get => _textOptionLottery;
            set
            {
                _textOptionLottery = value;
                OnPropertyChanged(nameof(TextOptionLottery));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void SetCoefficientsTextBlocks(CoefficientsDialog dialog)
        {
            TextOptionSure = "Click 'Sure' if you prefer to have for sure:\n";
            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionSure += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.ValuesToCompare[i] + "\n";

            TextOptionLottery = "\nClick 'Lottery' if you prefer to have with probability " + dialog.DisplayObject.P + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.BestValues[i] + "\n";

            TextOptionLottery += "\nOR with probability " + (1 - dialog.DisplayObject.P) + " these values:\n";

            for (var i = 0; i < dialog.DisplayObject.CriterionNames.Length; i++)
                TextOptionLottery += dialog.DisplayObject.CriterionNames[i] + " = " + dialog.DisplayObject.WorstValues[i] + "\n";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}