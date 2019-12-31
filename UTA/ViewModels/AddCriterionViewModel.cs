using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using UTA.Models.DataBase;

namespace UTA.ViewModels
{
    public class AddCriterionViewModel
    {
        public AddCriterionViewModel(Alternatives alternatives, Criteria criteria)
        {
            Alternatives = alternatives;
            Criteria = criteria;
            CriteriaTypeList = Enum.GetNames(typeof(Criterion.CriterionDirectionTypes)).ToList();
        }

        private Alternatives Alternatives { get; }

        private Criteria Criteria { get; }

        //todo combobox
        public List<string> CriteriaTypeList { get; set; }

        public bool AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            if (Criteria.AddCriterion(criterionName, criterionDescription, criterionDirection, linearSegments) is
                Criterion criterion)
            {
                Alternatives.AddNewCriterionToAlternatives(criterionName, null); //add crit values
                criterion.PropertyChanged += CriterionRenamed;
                return true;
            }
            else return false;
        }

        public void CriterionRenamed(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedExtendedEventArgs<string> eExtended = (PropertyChangedExtendedEventArgs<string>)e;
            Alternatives.UpdateCriteriaValueName(eExtended.OldValue, eExtended.NewValue);
        }

    }
}