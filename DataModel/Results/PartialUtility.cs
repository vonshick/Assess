using DataModel.Input;
using System.Collections.Generic;

namespace DataModel.Results
{
    public class PartialUtility
    {
        public PartialUtility(Criterion criterion, Dictionary<float, float> pointsValues)
        {
            Criterion = criterion;
            PointsValues = pointsValues;
        }

        public Criterion Criterion { get; set; }

        //Dict(point, value) - point from linear segment
        public Dictionary<float, float> PointsValues { get; set; }
    }
}