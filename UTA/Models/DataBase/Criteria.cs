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

        public void AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            CriteriaList.Add(new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments));
        }
    }
}