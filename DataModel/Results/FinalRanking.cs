using System.Collections.ObjectModel;
using DataModel.Structs;

namespace DataModel.Results
{
    public class FinalRanking
    {
        public FinalRanking()
        {
            FinalRankingCollection = new ObservableCollection<FinalRankingEntry>();
        }

        public FinalRanking(ObservableCollection<FinalRankingEntry> finalRankingCollection)
        {
            FinalRankingCollection = finalRankingCollection;
        }

        public ObservableCollection<FinalRankingEntry> FinalRankingCollection { get; set; }
    }
}