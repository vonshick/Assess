using System.Collections.Generic;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DisplayObject
    {
        //partial probability assessment dialog
        public float X;
        public Lottery Lottery;
        public List<PartialUtilityValues> PointsList;
        public Lottery EdgeValuesLottery; //lotteries comparison
        public Lottery ComparisonLottery; //lotteries comparison

        public float P; //lotteries comparison, coefficients dialog

        //coefficients dialog
        public float[] ValuesToCompare;
        public float[] WorstValues;
        public float[] BestValues;
        public string[] CriterionNames;

        public DisplayObject()
        {
            PointsList = new List<PartialUtilityValues>();
        }

        public DisplayObject(List<Criterion> criterionList, float[] valuesToCompare, float p)
        {
            ValuesToCompare = valuesToCompare;
            WorstValues = criterionList.Select(criterion => criterion.MinValue).ToArray();
            BestValues = criterionList.Select(criterion => criterion.MaxValue).ToArray();
            CriterionNames = criterionList.Select(criterion => criterion.Name).ToArray();
            P = p;
        }
    }
}