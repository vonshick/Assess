using System.Collections.ObjectModel;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Criteria
    {
        public string InputName { get; set; }
        public string InputDescription { get; set; }
        public int InputLinearSegments { get; set; }

        public ObservableCollection<Criterion> CriteriaList { get; set; }

        public Criteria()
        {
            CriteriaList = new ObservableCollection<Criterion>
            {
                new Criterion("variant 1", "desc", Criterion.Type.Gain, 2),
                new Criterion("variant 2", "desc", Criterion.Type.Cost, 3),
                new Criterion("variant 2", "desc", Criterion.Type.Ordinal, 2)
            };
        }

        public void AddCriterion(string typeString)
        {
            Criterion.Type type = (Criterion.Type)System.Enum.Parse(typeof(Criterion.Type), typeString);
            CriteriaList.Add(new Criterion(InputName, InputDescription, type, InputLinearSegments));
        }
    }
}