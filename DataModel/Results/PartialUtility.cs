using DataModel.Input;
using System.Collections.Generic;

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

        //Dict(point, value) - point from linear segment
        public List<PartialUtilityValues> PointsValues { get; set; }
    }
}