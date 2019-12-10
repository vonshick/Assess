using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class FinalRanking : Ranking
    {
        public FinalRanking(List<KeyValuePair<Alternative, int>> alternativeList, List<float> globalUtilities) : base(
            alternativeList)
        {
            GlobalUtilities = globalUtilities;
        }

        public List<float> GlobalUtilities { get; set; }
    }
}