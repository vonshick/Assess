namespace DataModel.Results
{
    public class PartialUtilityValues
    {
        public float MaxValue;
        public float MinValue;
        public float X;
        public float Y;

        public PartialUtilityValues(float x, float y)
        {
            X = x;
            Y = y;
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
        }

        public PartialUtilityValues(float point, float value, float minValue, float maxValue)
        {
            X = point;
            Y = value;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}