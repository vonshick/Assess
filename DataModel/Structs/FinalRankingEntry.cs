using DataModel.Input;

namespace DataModel.Structs
{
    public class FinalRankingEntry
    {
        public FinalRankingEntry(int position, Alternative alternative, float utility)
        {
            Position = position;
            Alternative = alternative;
            Utility = utility;
        }

        public int Position { get; set; }
        public Alternative Alternative { get; set; }
        public float Utility { get; set; }
    }
}