using DataModel.Input;

namespace DataModel.Structs
{
    public struct FinalRankingEntry
    {
        public int Position { get; set; }
        public Alternative Alternative { get; set; }
        public float Utility { get; set; }
        public bool IsInReferenceRanking { get; set; }

        public FinalRankingEntry(int position, Alternative alternative, float utility, bool isInReferenceRanking)
        {
            Position = position;
            Alternative = alternative;
            Utility = utility;
            IsInReferenceRanking = isInReferenceRanking;
        }
    }
}