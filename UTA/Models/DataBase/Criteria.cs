using System.Collections.ObjectModel;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Criteria
    {
        public Criteria()
        {
            CriteriaCollection = new ObservableCollection<Criterion>();
        }

        public ObservableCollection<Criterion> CriteriaCollection { get; set; }

        public Criterion Placeholder { get; set; }

        public Criterion AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            var criterion = new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments);
            if (CriteriaCollection.Contains(criterion)) return null;
            CriteriaCollection.Add(criterion);
            return criterion;
        }

        public void RemoveCriterion(Criterion criterion)
        {
            //todo update alternatives!
            CriteriaCollection.Remove(criterion);
        }

        public void AddCriterion(Criterion criterion)
        {
            CriteriaCollection.Add(criterion);
        }

        public Criterion SaveCurrentPlaceholderToCollection()
        {
            AddCriterion(Placeholder);
            return Placeholder;
        }

        public void AddNewPlaceholderToCollection()
        {
            Placeholder = AddCriterion("name", "description", "", 1);
        }

        public void RemovePlaceholderFromCollection()
        {
            CriteriaCollection.Remove(Placeholder);
        }

        //todo handle name changes: change column bindings and headers, CriteriaValues in alternatives, etc
    }
}