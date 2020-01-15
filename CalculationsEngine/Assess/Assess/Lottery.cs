using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class Lottery
    {
        public PartialUtilityValues LowerUtilityValue;
        public double P;
        public PartialUtilityValues UpperUtilityValue;

        public Lottery(PartialUtilityValues lowerUtilityValue, PartialUtilityValues upperUtilityValue)
        {
            LowerUtilityValue = lowerUtilityValue;
            UpperUtilityValue = upperUtilityValue;
        }

        public void SetProbability(double p)
        {
            P = p;
        }

        public double NewPointUtility()
        {
            return P * UpperUtilityValue.Y + (1 - P) * LowerUtilityValue.Y;
        }
    }
}