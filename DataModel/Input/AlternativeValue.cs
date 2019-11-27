namespace DataModel.Input
{
    public class AlternativeValue
    {
        public AlternativeValue()
        {
        }

        public AlternativeValue(Criterion criterion, float value)
        {
            CriterionObj = criterion;
            Value = value;
        }

        public Criterion CriterionObj { get; set; }
        public float Value { get; set; }
    }
}