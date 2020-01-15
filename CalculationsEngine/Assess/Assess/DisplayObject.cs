using System.Collections.Generic;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DisplayObject
    {
        //partial probability assessment dialog
        public double X;
        public Lottery Lottery;
        public List<PartialUtilityValues> PointsList;
        public Lottery EdgeValuesLottery; //lotteries comparison
        public Lottery ComparisonLottery; //lotteries comparison

        public double P; //lotteries comparison, coefficients dialog

        //coefficients dialog
        public double[] ValuesToCompare;
        public double[] WorstValues;
        public double[] BestValues;
        public string[] CriterionNames;

        public DisplayObject()
        {
            PointsList = new List<PartialUtilityValues>();
        }

        public DisplayObject(List<Criterion> criterionList, double[] valuesToCompare, double p)
        {
            ValuesToCompare = valuesToCompare;
            WorstValues = criterionList.Select(criterion => criterion.MinValue).ToArray();
            BestValues = criterionList.Select(criterion => criterion.MaxValue).ToArray();
            CriterionNames = criterionList.Select(criterion => criterion.Name).ToArray();
            P = p;
        }
    }
}