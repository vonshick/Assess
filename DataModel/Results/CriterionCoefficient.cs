namespace DataModel.Results
{
    public struct CriterionCoefficient
    {
        public string CriterionName { get; set; }
        public double Coefficient { get; set; }

        public CriterionCoefficient(string criterionName, double coefficient)
        {
            CriterionName = criterionName;
            Coefficient = coefficient;
        }
    }
}