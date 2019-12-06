using System.Collections.ObjectModel;
using System.ComponentModel;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Criteria
    {
        public ObservableCollection<Criterion> CriteriaCollection { get; set; }

        public Criteria()
        {
            CriteriaCollection = new ObservableCollection<Criterion>();
        }

        public bool AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            Criterion criterion = new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments);
            if (CriteriaCollection.Contains(criterion)) return false;
            CriteriaCollection.Add(criterion);
            return true;
        }
    }
}