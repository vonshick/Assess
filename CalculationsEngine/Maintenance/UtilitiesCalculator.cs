// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CalculationsEngine.Models;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Maintenance
{
    public class UtilitiesCalculator
    {
        private readonly List<Alternative> _alternativesList;
        private List<AlternativeUtility> _alternativesUtilitiesList;
        private BairstowSolver _bairstowSolver;
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
                    pointsList.Add(new PartialUtilityValues(criterion.MinValue, 1));
                    pointsList.Add(new PartialUtilityValues(criterion.MaxValue, 0));
                }
                else
                {
                    pointsList.Add(new PartialUtilityValues(criterion.MinValue, 0));
                    pointsList.Add(new PartialUtilityValues(criterion.MaxValue, 1));
                }

                partialUtilityFunctions.Add(new PartialUtility(criterion, pointsList));
            }

            return partialUtilityFunctions;
        }

        private void SetScalingCoefficient()
        {
            _bairstowSolver = new BairstowSolver();
            var kCoefficients = Results.CriteriaCoefficients.Select(o => o.Coefficient).ToList();
            var solverResults = _bairstowSolver.GetSolverResults(kCoefficients);
            Results.K = solverResults.K;
            // Results.Formula = solverResults.Formula;
        }

        public void CalculateGlobalUtilities()
        {
            _alternativesUtilitiesList.Clear();
            // if sum of k weights equals one than our utility function is simply linear
            // U = sum(k_i * u_i)
            bool isCoefficientSumOne = Results.CriteriaCoefficients.Sum(o => o.Coefficient) == 1;
            foreach (var alternative in _alternativesList)
            {
                double product = 1, sum = 0;

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
                                u = a * (double)criterionValue.Value + b;
                            }

                    if (!isCoefficientSumOne)
                        product *= (double) Results.K * k * u + 1;
                    else
                        sum += k * u;
                }

                var utility = isCoefficientSumOne ? sum : (product - 1) / (double)Results.K;

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