namespace Assess
{
    public class Criterion
    {
        public Criterion() { }
        public Criterion(string name, string criterionDirection, float minValue, float maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Name = name;
            CriterionDirection = criterionDirection;
        }

        public string Name;
        public string CriterionDirection { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
    }
}
