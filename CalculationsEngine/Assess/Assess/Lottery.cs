
namespace CalculationsEngine.Assess.Assess
{
    public class Lottery
    {
        public float P;
        public Point UpperUtilityValue;
        public Point LowerUtilityValue;

        public Lottery(Point lowerUtilityValue, Point upperUtilityValue)
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
            return P * UpperUtilityValue.U + (1 - P) * LowerUtilityValue.U;
        }
    }
}
