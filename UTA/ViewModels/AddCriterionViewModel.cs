using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using DataModel.Input;
using UTA.Models.DataBase;
using UTA.Views;

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
        //        public static Dictionary<string, string> CriterionDirectionTypes = new Dictionary<string, string>
        //        {
        //            { "g", "Gain" },
        //            { "c", "Cost" },
        //            { "o", "Ordinal" }
        //        };


        public bool AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            if (!Criteria.AddCriterion(criterionName, criterionDescription, criterionDirection, linearSegments))
                return false;
            MainViewModel.Alternatives.AddNewCriterionToAlternatives(criterionName, 0.1f, MainViewModel.GenerateAlternativesTable); //add crit values
            MainViewModel.GenerateCriteriaTable();
            MainViewModel.GenerateAlternativesTable();
            return true;
        }


    }
}