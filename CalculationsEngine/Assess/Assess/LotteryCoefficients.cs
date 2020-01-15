using System.Collections.Generic;
using System.Linq;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class LotteryCoefficients
    {
        public LotteryCoefficients(List<Criterion> criterionList, double[] valuesToCompare, double p)
        {
            ValuesToCompare = valuesToCompare;
            WorstValues = criterionList.Select(criterion => criterion.MinValue).ToArray();
            BestValues = criterionList.Select(criterion => criterion.MaxValue).ToArray();
            CriterionNames = criterionList.Select(criterion => criterion.Name).ToArray();
            P = p;
        }

        public double[] ValuesToCompare;
        public double[] WorstValues;
        public double[] BestValues;
        public string[] CriterionNames;
        public double P;
    }
}
