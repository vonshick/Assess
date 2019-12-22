using DataModel.Input;
using System.Collections.ObjectModel;

namespace UTA.Models.DataBase
{
    public class Criteria
    {
        public ObservableCollection<Criterion> CriteriaCollection { get; set; }

        public Criteria()
        {
            CriteriaCollection = new ObservableCollection<Criterion>();
        }

        public Criterion AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            Criterion criterion = new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments);
            if (CriteriaCollection.Contains(criterion)) return null;
            CriteriaCollection.Add(criterion);
            return criterion;
        }

        //todo handle name changes: change column bindings and headers, CriteriaValues in alternatives, etc
    }
}