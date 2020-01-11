
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class Lottery
    {
        public float P;
        public PartialUtilityValues UpperUtilityValue;
        public PartialUtilityValues LowerUtilityValue;

        public Lottery(PartialUtilityValues lowerUtilityValue, PartialUtilityValues upperUtilityValue)
        {
            LowerUtilityValue = lowerUtilityValue;
            UpperUtilityValue = upperUtilityValue;
        }

        public void SetProbability(float p)
        {
            P = p;
        }

        public float NewPointUtility()
        {
            return P * UpperUtilityValue.Y + (1 - P) * LowerUtilityValue.Y;
        }
    }
}
