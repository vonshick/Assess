using System.Collections.ObjectModel;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Criteria
    {
        public ObservableCollection<Criterion> CriteriaList { get; set; }

        public Criteria()
        {
            CriteriaList = new ObservableCollection<Criterion>();
        }

        public bool AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            Criterion criterion = new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments);
            if (CriteriaList.Contains(criterion)) return false;
            CriteriaList.Add(criterion);
            return true;
        }
    }
}