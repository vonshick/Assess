using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Input;
using UTA.Models.DataBase;

namespace UTA.ViewModels
{
    public class AddCriterionViewModel
    {
        public MainViewModel MainViewModel { get; set; }

        public AddCriterionViewModel()
        {
            CriteriaTypeList = Enum.GetNames(typeof(Criterion.CriterionDirectionTypes)).ToList();
        }

        public Criteria Criteria { get; set; }

        public List<string> CriteriaTypeList { get; set; }


        public bool AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            if (Criteria.AddCriterion(criterionName, criterionDescription, criterionDirection, linearSegments) is
                Criterion criterion)
            {
                MainViewModel.Alternatives.AddNewCriterionToAlternatives(criterionName, 0.1f); //add crit values
                criterion.PropertyChanged += MainViewModel.CriterionRenamed;
                return true;
            }
            else return false;
        }


    }
}