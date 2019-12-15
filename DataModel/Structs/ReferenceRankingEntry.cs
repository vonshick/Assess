using DataModel.Input;

namespace DataModel.Structs
{
    public class ReferenceRankingEntry
    {
        public int Rank { get; set; }
        public Alternative Alternative { get; set; }

        public ReferenceRankingEntry(int rank, Alternative alternative)
        {
            Rank = rank;
            Alternative = alternative;
        }
    }
}