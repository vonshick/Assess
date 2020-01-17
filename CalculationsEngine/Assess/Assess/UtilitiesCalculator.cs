using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class UtilitiesCalculator
    {
        private readonly List<Alternative> _alternativesList;
        private BaristowSolver _baristowSolver;
        private List<AlternativeUtility> _alternativesUtilitiesList;

        public Results Results;

        public UtilitiesCalculator(List<Alternative> alternativesList, Results results, List<Criterion> criteriaList)
        {
            Results = results;
            Results.PartialUtilityFunctions = InitPartialUtilityFunctions(criteriaList);
            _alternativesUtilitiesList = new List<AlternativeUtility>();
            _alternativesList = alternativesList;
            setScalingCoefficient(Results.CriteriaCoefficients);
        }

        private List<PartialUtility> InitPartialUtilityFunctions(List<Criterion> criteriaList)
        {
            List<PartialUtility> partialUtilityFunctions = new List<PartialUtility>();

            foreach (Criterion criterion in criteriaList)
            {
                var pointsList = new List<PartialUtilityValues>();
                if (criterion.CriterionDirection.Equals("Cost"))
                {
                    pointsList.Add(new PartialUtilityValues(criterion.MaxValue, 0));
                    pointsList.Add(new PartialUtilityValues(criterion.MinValue, 1));
                }
                else
                {
                    pointsList.Add(new PartialUtilityValues(criterion.MaxValue, 1));
                    pointsList.Add(new PartialUtilityValues(criterion.MinValue, 0));
                }

                partialUtilityFunctions.Add(new PartialUtility(criterion, pointsList));
            }

            return partialUtilityFunctions;
        }

        private void setScalingCoefficient(List<CriterionCoefficient> criteriaCoefficientsList)
        {
            _baristowSolver = new BaristowSolver();
            var kCoefficients = criteriaCoefficientsList.Select(o => o.Coefficient).ToList();
            Results.K = _baristowSolver.GetScalingCoefficient(kCoefficients);
        }

        public void CalculateGlobalUtilities()
        {
            foreach (var alternative in _alternativesList)
            {
                double product = 1;
                foreach (var criterionValue in alternative.CriteriaValuesList)
                {
                    var points = Results.PartialUtilityFunctions.Find(o => o.Criterion.Name == criterionValue.Name).PointsValues;
                    points = points.OrderBy(o => o.X).ToList();
                    var k = Results.CriteriaCoefficients.Find(element => element.CriterionName == criterionValue.Name).Coefficient;
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

                    product *= (double)Results.K * k * u + 1;
                }

                var utility = (product - 1) / (double) Results.K;

                _alternativesUtilitiesList.Add(new AlternativeUtility(alternative, (double) utility));
            }

            UpdateFinalRanking();
        }

        private void UpdateFinalRanking()
        {
            _alternativesUtilitiesList = _alternativesUtilitiesList.OrderBy(o => o.Utility).ToList();
            ObservableCollection<FinalRankingEntry> finalRankingEntries = new ObservableCollection<FinalRankingEntry>();

            for (var i = 0; i < _alternativesUtilitiesList.Count; i++)
                finalRankingEntries.Add(new FinalRankingEntry(_alternativesUtilitiesList.Count - i,
                    _alternativesUtilitiesList[i].Alternative, _alternativesUtilitiesList[i].Utility));

            Results.FinalRanking.FinalRankingCollection = finalRankingEntries;
        }

    }
}