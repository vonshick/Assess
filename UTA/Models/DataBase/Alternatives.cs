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
        public Criteria Criteria { get; set; }

        public Alternatives()
        {
            AlternativesCollection = new ObservableCollection<Alternative>();
        }

        public Alternatives(Criteria criteria)
        {
            Criteria = criteria;
            AlternativesCollection = new ObservableCollection<Alternative>();
        }

        public Alternative AddAlternative(string name, string description, PropertyChangedEventHandler criterionValuePropertyChangedEventHandler)
        {
            Alternative alternative = new Alternative(name, description, Criteria.CriteriaCollection, criterionValuePropertyChangedEventHandler);
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
