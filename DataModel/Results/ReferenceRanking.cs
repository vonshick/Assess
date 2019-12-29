using System.Collections.Generic;
using DataModel.Structs;

namespace DataModel.Results
{
    public class ReferenceRanking
    {
        public ReferenceRanking(List<ReferenceRankingEntry> referenceRankingList)
        {
            ReferenceRankingList = referenceRankingList;
        }

        public List<ReferenceRankingEntry> ReferenceRankingList { get; set; }
    }
}