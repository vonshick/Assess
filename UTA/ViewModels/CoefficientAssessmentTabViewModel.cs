using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
        private readonly Action<List<CriterionCoefficient>> _showPartialUtilityTabs;
        private int _currentCriterionIndex;


        public CoefficientAssessmentTabViewModel(List<Criterion> criteriaCollection,
            Action<List<CriterionCoefficient>> showPartialUtilityTabs)
        {
            Name = "Dialogue - Scaling Coefficient";
            _criteriaCollection = criteriaCollection;
            _showPartialUtilityTabs = showPartialUtilityTabs;
            Dialog = new CoefficientsDialog(criteriaCollection);
            Dialog.SetInitialValues(_criteriaCollection[_currentCriterionIndex = 0]);
        }


        public CoefficientsDialog Dialog { get; set; }
        public Criterion CurrentCriterion => _criteriaCollection[_currentCriterionIndex];

        public event PropertyChangedEventHandler PropertyChanged;


        [UsedImplicitly]
        public void CertaintyOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(1);
        }

        [UsedImplicitly]
        public void LotteryOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(1);
        }

        [UsedImplicitly]
        public void IndifferentOptionChosen(object sender, RoutedEventArgs e)
        {
            Dialog.ProcessDialog(3);
            if (_currentCriterionIndex < _criteriaCollection.Count - 1)
            {
                Dialog.SetInitialValues(_criteriaCollection[++_currentCriterionIndex]);
                OnPropertyChanged(nameof(CurrentCriterion));
            }
            else
            {
                _showPartialUtilityTabs(Dialog.CriteriaCoefficientsList);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}