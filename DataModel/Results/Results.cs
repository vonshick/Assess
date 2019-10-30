using System.Collections.Generic;

namespace DataModel.Results
{
    //very general class concept for now
    public class Results
    {
        public FinalRanking FinalRanking { get; set; }

        public List<PartialUtilityFunction> PartialUtilityFunctions { get; set; }

        public float KendallCoefficient { get; set; }

    }
}
