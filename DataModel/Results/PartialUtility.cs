using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class PartialUtility
    {
        public PartialUtility(Criterion criterion, List<PartialUtilityValues> pointsValues)
        {
            Criterion = criterion;
            PointsValues = pointsValues;
        }

        public Criterion Criterion { get; set; }
        public List<PartialUtilityValues> PointsValues { get; set; }
    }
}