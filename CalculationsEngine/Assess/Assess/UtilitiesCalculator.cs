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
        private List<AlternativeUtility> _alternativesUtilitiesList;
        private BaristowSolver _baristowSolver;
        public Results Results;


        public UtilitiesCalculator(List<Alternative> alternativesList, Results results, List<Criterion> criteriaList)
        {
            Results = results;
            if (Results.PartialUtilityFunctions.Count <= 0) Results.PartialUtilityFunctions = InitPartialUtilityFunctions(criteriaList);
            _alternativesUtilitiesList = new List<AlternativeUtility>();
            _alternativesList = alternativesList;
            SetScalingCoefficient();
        }


        private List<PartialUtility> InitPartialUtilityFunctions(List<Criterion> criteriaList)
        {
            var partialUtilityFunctions = new List<PartialUtility>();

            foreach (var criterion in criteriaList)
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

        private void SetScalingCoefficient()
        {
            _baristowSolver = new BaristowSolver();
            var kCoefficients = Results.CriteriaCoefficients.Select(o => o.Coefficient).ToList();
            Results.K = _baristowSolver.GetScalingCoefficient(kCoefficients);
        }

        public void CalculateGlobalUtilities()
        {
            _alternativesUtilitiesList.Clear();

            foreach (var alternative in _alternativesList)
            {
                double product = 1;
                foreach (var criterionValue in alternative.CriteriaValuesList)
                {
                    var points = Results.PartialUtilityFunctions.Find(o => o.Criterion.Name == criterionValue.Name).PointsValues;
                    points = points.OrderBy(o => o.X).ToList();
                    var k = Results.CriteriaCoefficients.First(element => element.CriterionName == criterionValue.Name).Coefficient;
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

                    product *= (double) Results.K * k * u + 1;
                }

                var utility = (product - 1) / (double) Results.K;

                _alternativesUtilitiesList.Add(new AlternativeUtility(alternative, utility));
            }

            UpdateFinalRanking();
        }

        private void UpdateFinalRanking()
        {
            _alternativesUtilitiesList = _alternativesUtilitiesList.OrderByDescending(o => o.Utility).ToList();
            var finalRankingEntries = new ObservableCollection<FinalRankingEntry>();

            for (var i = 0; i < _alternativesUtilitiesList.Count; i++)
                finalRankingEntries.Add(new FinalRankingEntry(i + 1,
                    _alternativesUtilitiesList[i].Alternative, _alternativesUtilitiesList[i].Utility));

            Results.FinalRanking.FinalRankingCollection = finalRankingEntries;
        }

        public void UpdateScalingCoefficients()
        {
            SetScalingCoefficient();
            CalculateGlobalUtilities();
        }
    }
}