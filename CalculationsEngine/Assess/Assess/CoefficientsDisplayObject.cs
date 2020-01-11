using System.Collections.Generic;
using System.Linq;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class CoefficientsDisplayObject
    {
        public CoefficientsDisplayObject(List<Criterion> criterionList, float[] valuesToCompare, float p)
        {
            ValuesToCompare = valuesToCompare;
            WorstValues = criterionList.Select(criterion => criterion.MinValue).ToArray();
            BestValues = criterionList.Select(criterion => criterion.MaxValue).ToArray();
            CriterionNames = criterionList.Select(criterion => criterion.Name).ToArray();
            P = p;
        }

        public float[] ValuesToCompare;
        public float[] WorstValues;
        public float[] BestValues;
        public string[] CriterionNames;
        public float P;
    }
}
