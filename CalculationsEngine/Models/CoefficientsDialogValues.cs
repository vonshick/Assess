namespace CalculationsEngine.Models
{
    public class CoefficientsDialogValues
    {
        public CoefficientsDialogValues(double valueToCompare, double bestValue, double worstValue, string criterionName)
        {
            ValueToCompare = valueToCompare;
            BestValue = bestValue;
            WorstValue = worstValue;
            CriterionName = criterionName;
        }

        public double ValueToCompare { get; set; }
        public double BestValue { get; set; }
        public double WorstValue { get; set; }
        public string CriterionName { get; set; }
    }
}