using System.Collections.Generic;
using DataModel.Input;
using DataModel.Structs;

namespace DataModel.Results
{
    public class FinalRanking
    {
        public FinalRanking(List<FinalRankingEntry> finalRankingList)
        {
            FinalRankingList = finalRankingList;
        }
        public List<FinalRankingEntry> FinalRankingList { get; set; }
    }
}