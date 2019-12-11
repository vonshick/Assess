using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class Ranking
    {
        public Ranking()
        {
        }

        public Ranking(List<KeyValuePair<Alternative, int>> alternativeList)
        {
            AlternativeList = alternativeList;
        }

        /// <summary> order of variants in list defines reference ranking order </summary>
        public List<KeyValuePair<Alternative, int>> AlternativeList { get; set; }

        public void AddVariant(Alternative alternative, int rank)
        {
            AlternativeList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
        }
    }
}