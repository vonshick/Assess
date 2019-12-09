using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public ObservableCollection<Alternative> AlternativesCollection { get; set; }

        public Alternatives()
        {
            AlternativesCollection = new ObservableCollection<Alternative>();
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
        }

        public Alternative AddAlternative(string Name, string Description)
        {
            Alternative alternative = new Alternative(Name, Description);
            AlternativesCollection.Add(alternative);
            return alternative;
        }

        public void AddNewCriterionToAlternatives(string name, string value, PropertyChangedEventHandler criterionValuePropertyChangedEventHandler)
        {
            foreach (var alternative in AlternativesCollection)
            {
                CriterionValue criterionValue = new CriterionValue(name, value);
                criterionValue.PropertyChanged += criterionValuePropertyChangedEventHandler;
                alternative.AddCriterionValue(criterionValue);
            }
        }
    }
}
