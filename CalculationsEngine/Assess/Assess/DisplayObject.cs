using System.Collections.Generic;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DisplayObject
    {
        public double[] BestValues;
        public Lottery ComparisonLottery; //lotteries comparison
        public string[] CriterionNames;
        public Lottery EdgeValuesLottery; //lotteries comparison
        public Lottery Lottery;

        public double P; //lotteries comparison, coefficients dialog
        public List<PartialUtilityValues> PointsList;

        //coefficients dialog
        public double[] ValuesToCompare;

        public double[] WorstValues;

        //partial probability assessment dialog
        public double X;

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