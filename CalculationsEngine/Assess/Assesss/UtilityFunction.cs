using System.Collections.Generic;

namespace Assess
{
    public class UtilityFunction
    {
        public UtilityFunction(string criterionName, List<Point> pointsList)
        {
            CriterionName = criterionName;
            PointsList = pointsList;
        }

        public string CriterionName;
        public List<Point> PointsList;
    }
}
