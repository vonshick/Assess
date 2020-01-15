using System.Collections.Generic;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class UtilitiesCalculator
    {
        private readonly List<Alternative> _alternativesList;
        private readonly List<CriterionCoefficient> _criteriaCoefficientsList;
        private readonly List<PartialUtility> _partialUtilitiesList;
        private BaristowSolver _baristowSolver;
        public List<AlternativeUtility> AlternativesUtilitiesList;
        private double K;

        public UtilitiesCalculator(List<Alternative> alternativesList,
            List<PartialUtility> partialUtilitiesList, List<CriterionCoefficient> criteriaCoefficientsList)
        {
            AlternativesUtilitiesList = new List<AlternativeUtility>();
            _alternativesList = alternativesList;
            _partialUtilitiesList = partialUtilitiesList;
            _criteriaCoefficientsList = criteriaCoefficientsList;
            setScalingCoefficient(criteriaCoefficientsList);
        }

        private void setScalingCoefficient(List<CriterionCoefficient> criteriaCoefficientsList)
        {
            _baristowSolver = new BaristowSolver();
            var kCoefficients = criteriaCoefficientsList.Select(o => o.Coefficient).ToList();
            K = _baristowSolver.GetScalingCoefficient(kCoefficients);
        }

        public void CalculateGlobalUtilities()
        {
            foreach (var alternative in _alternativesList)
            {
                double product = 1;
                foreach (var criterionValue in alternative.CriteriaValuesList)
                {
                    var points = _partialUtilitiesList.Find(o => o.Criterion.Name == criterionValue.Name).PointsValues;
                    points = points.OrderBy(o => o.X).ToList();
                    var k = _criteriaCoefficientsList.Find(element => element.CriterionName == criterionValue.Name).Coefficient;
                    double u = 1;

                    if (points[0].X == criterionValue.Value)
                        u = points[0].Y;
                    else
                        for (var i = 0; i < points.Count - 1; i++)
                            if (criterionValue.Value > points[i].X && criterionValue.Value <= points[i + 1].X)
                            {
                                var a = (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X);
                                var b = points[i].Y - a * points[i].X;
                                u = a * (double) criterionValue.Value + b;
                            }

                    product *= K * k * u + 1;
                }

                var utility = (product - 1) / K;

                AlternativesUtilitiesList.Add(new AlternativeUtility(alternative, utility));
            }
        }
    }
}