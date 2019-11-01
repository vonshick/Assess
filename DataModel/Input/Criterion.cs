namespace DataModel.Input
{
    public class Criterion
    {
        public Criterion(string name, Type criterionType)
        {
            Name = name;
            CriterionType = criterionType;
        }

        public enum Type { Gain, Cost };
        public string Name { get; set; }
        public string Description { get; set; }
        public Type CriterionType { get; set; }
        public int LinearSegments { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }


    }
}
