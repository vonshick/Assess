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

        public void AddCriterion(string typeString, string inputName, string inputDescription, int inputLinearSegments)
        {
            Criterion.Type type = (Criterion.Type)System.Enum.Parse(typeof(Criterion.Type), typeString);
            CriteriaList.Add(new Criterion(inputName, inputDescription, type, inputLinearSegments));
        }
    }
}