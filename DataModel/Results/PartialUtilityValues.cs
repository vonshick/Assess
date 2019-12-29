namespace DataModel.Results
{
    public class PartialUtilityValues
    {
        public float MaxValue;
        public float MinValue;
        public float Point;
        public float Value;

        public PartialUtilityValues(float point, float value)
        {
            Point = point;
            Value = value;
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
        }

        public PartialUtilityValues(float point, float value, float minValue, float maxValue)
        {
            Point = point;
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}