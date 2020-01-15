namespace DataModel.Results
{
    public class PartialUtilityValues
    {
        public double MaxValue;
        public double MinValue;
        public double X;
        public double Y;

        public PartialUtilityValues(double x, double y)
        {
            X = x;
            Y = y;
            MinValue = double.MaxValue;
            MaxValue = double.MinValue;
        }

        public PartialUtilityValues(double point, double value, double minValue, double maxValue)
        {
            X = point;
            Y = value;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}