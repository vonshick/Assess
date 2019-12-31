using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using UTA.Models.DataBase;
using UTA.Models.Tab;
using UTA.Views;

namespace UTA.ViewModels
{
    public class CriteriaTabViewModel : Tab
    {
        public CriteriaTabViewModel(Criteria criteria, Alternatives alternatives)
        {
            Name = "Criteria";
            Criteria = criteria;
            Alternatives = alternatives;
        }

        public Criteria Criteria { get; }
        private Alternatives Alternatives { get; }

        public void CriterionRenamed(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedExtendedEventArgs<string> eExtended = (PropertyChangedExtendedEventArgs<string>) e;
            Alternatives.UpdateCriteriaValueName(eExtended.OldValue, eExtended.NewValue);
        }

        public void AddPlaceholder()
        {
            Criteria.AddNewPlaceholderToCollection();
        }

        public void SaveCurrentPlaceholder()
        {
            //remove and save again will regenearte row and remove it's styling as placeholder
            RemovePlaceholder();
            var savedCriterion = Criteria.SaveCurrentPlaceholderToCollection();
            Alternatives.AddNewCriterionToAlternatives(savedCriterion.Name, null); //add crit values
            savedCriterion.PropertyChanged += CriterionRenamed;
            //add new placeholder
            AddPlaceholder();
        }

        public void RemovePlaceholder()
        {
            Criteria.RemovePlaceholderFromCollection();
        }

        //todo remove
        public void ShowAddCriterionDialog()
        {
            var addCriterionViewModel = new AddCriterionViewModel(Alternatives, Criteria);
            var addCriterionWindow = new AddCriterionView(addCriterionViewModel);
            addCriterionWindow.ShowDialog();
        }
    }
}