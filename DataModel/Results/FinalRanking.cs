using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class FinalRanking : Ranking
    {
        public FinalRanking(List<Alternative> variantsList, List<float> globalUtilities) : base(variantsList)
        {
            GlobalUtilities = globalUtilities;
        }

        public List<float> GlobalUtilities { get; set; }

    }
}
