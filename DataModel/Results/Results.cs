using System.Collections.Generic;

namespace DataModel.Results
{
    //very general class concept for now
    public class Results
    {
        public Results()
        {
            FinalRanking = new FinalRanking();
        }


        public FinalRanking FinalRanking { get; set; }

        public List<PartialUtility> PartialUtilityFunctions { get; set; }

        public float KendallCoefficient { get; set; }


        public void Reset()
        {
            FinalRanking.FinalRankingCollection.Clear();
            PartialUtilityFunctions?.Clear();
            KendallCoefficient = 0;
        }
    }
}