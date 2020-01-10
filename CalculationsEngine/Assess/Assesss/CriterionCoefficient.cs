namespace Assess
{
    public class CriterionCoefficient
    {
        public CriterionCoefficient(string criterionName, float coefficient)
        {
            CriterionName = criterionName;
            Coefficient = coefficient;
        }

        public string CriterionName;
        public float Coefficient;
    }
}
