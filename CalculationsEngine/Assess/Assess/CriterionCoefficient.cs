namespace CalculationsEngine.Assess.Assess
{
    public struct CriterionCoefficient
    {
        public string CriterionName;
        public float Coefficient;

        public CriterionCoefficient(string criterionName, float coefficient)
        {
            CriterionName = criterionName;
            Coefficient = coefficient;
        }
    }
}
