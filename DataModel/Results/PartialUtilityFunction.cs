using DataModel.Input;
using System.Collections.Generic;

namespace DataModel.Results
{
    public class PartialUtilityFunction
    {
        public Criterion Criterion { get; set; }

        //Dict(point, value) - point from linear segment
        public Dictionary<float, float> PointsValues { get; set; }

        public PartialUtilityFunction()
        {
        }
    }
}
