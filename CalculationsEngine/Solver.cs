using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;

namespace CalculationsEngine
{
    public class Solver
    {
        private readonly List<Criterion> criteriaList;
        private readonly List<int> equals;
        private double[] arrayOfValues;
        private List<Alternative> arternativesList;
        private int[] basicVariables;
        private int criterionFieldsCount;
        private List<Alternative> otherAlternatives;
        private double[,] otherAlternativesMatrix;
        //private ReferenceRanking referenceRanking;
        //private List<List<Alternative>> referenceRankingList;
        private double[,] restrictionsMatrix;
        private Dictionary<double, double> solution;
        private double[,] transientMatrix;
        private List<KeyValuePair<Alternative, int>> variantsList;
        private double[] vectorCj;

        public Solver()
        {
        }

        public Solver(List<List<Alternative>> referenceRankingList, List<Criterion> criteriaList, List<Alternative> otherAlternatives, Results results,
            bool preserveKendallCoefficient = false, double deltaThreshold = 0.05, double epsilonThreshold = 0.0000001)
        {
            DeltaThreshold = deltaThreshold;
            EpsilonThreshold = epsilonThreshold;
            PreserveKendallCoefficient = preserveKendallCoefficient;
            NumberOfIteration = 250;
            this.criteriaList = criteriaList;
            Result = results;
            //this.referenceRanking = referenceRanking;
            //this.referenceRankingList = referenceRankingList;
            variantsList = new List<KeyValuePair<Alternative, int>>();
            arternativesList = new List<Alternative>();
            for (var rank = 0; rank < referenceRankingList.Count; rank++)
                foreach (var alternative in referenceRankingList[rank])
                {
                    variantsList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
                    arternativesList.Add(alternative);
                }

            //for (var rank = 0; rank < referenceRanking.RankingsCollection.Count; rank++)
            //    foreach (var alternative in referenceRanking.RankingsCollection[rank])
            //    {
            //        variantsList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
            //        arternativesList.Add(alternative);
            //}

            var cfc = 0;
            foreach (var criterion in criteriaList) cfc += criterion.LinearSegments;
            criterionFieldsCount = cfc;
            equals = new List<int>();
            for (var i = 0; i < arternativesList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);
            this.otherAlternatives = new List<Alternative>();
            this.otherAlternatives = otherAlternatives;
            otherAlternativesMatrix = CreateMatrix(otherAlternatives);
        }

        public Results Result { get; set; }
        public int NumberOfIteration { get; set; }
        public double DeltaThreshold { get; set; }
        public double EpsilonThreshold { get; set; }
        public bool PreserveKendallCoefficient { get; set; }

        public void Calculate()
        {
            var heightOfPrimaryMatrix = variantsList[0].Key.CriteriaValuesList.Count;
            var height = variantsList.Count;
            var width = WidthOfSimplexMatrix(variantsList[0].Key);
            List<PartialUtility> partialUtilityList;

            transientMatrix = CreateMatrix(arternativesList);
            var createdSimplexMatrix = CreateSimplexMatrix(height, width, transientMatrix);

            var simplex =
                new Simplex(createdSimplexMatrix, basicVariables, vectorCj);
            simplex.Run(NumberOfIteration);
            solution = simplex.solution;
            (partialUtilityList, arrayOfValues) = MakePartialUtilityFunction(solution);
            var finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            var finalReferenceRanking = new FinalRanking(finalReferenceList);
            var recalculate = false;
            var widthWithoutSlack = width - height - equals.Count;
            for (var i = widthWithoutSlack; i < width; i++)
                if (solution.ContainsKey(i))
                    recalculate = true;

            if (recalculate)
            {
                var recalculatedMatrix = RecreateMatrix(finalReferenceRanking);
                restrictionsMatrix = CalculateRestrictions(recalculatedMatrix, finalReferenceRanking);
            }
            else
            {
                restrictionsMatrix = CalculateRestrictions(transientMatrix, finalReferenceRanking);
            }

            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(restrictionsMatrix, arrayOfValues);
            var count = 0;
            for (var numOfCriterion = 0; numOfCriterion < partialUtilityList.Count; numOfCriterion++)
            {
                partialUtilityList[numOfCriterion].PointsValues[0].MinValue = 0;
                partialUtilityList[numOfCriterion].PointsValues[0].MaxValue = 0;
                for (var i = 1; i < partialUtilityList[numOfCriterion].PointsValues.Count; i++)
                {
                    partialUtilityList[numOfCriterion].PointsValues[i].MinValue = (float) minArray[count];
                    partialUtilityList[numOfCriterion].PointsValues[i].MaxValue = (float) maxArray[count];
                    count++;
                }
            }

            var tau = CalculateKendallCoefficient(finalReferenceRanking);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderByDescending(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.GetLength(0); i++) allFinalRankingEntry[i].Position = i + 1;

            Result.PartialUtilityFunctions = partialUtilityList;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        public void LoadState(List<PartialUtility> partialUtilityList, List<List<Alternative>> referenceRankingList,
            List<Alternative> notRankedAlternatives, Results results)
        {
            Result = results;
            //this.referenceRanking = referenceRanking;
            //this.referenceRankingList = referenceRankingList;
            variantsList = new List<KeyValuePair<Alternative, int>>();
            arternativesList = new List<Alternative>();
            for (var rank = 0; rank < referenceRankingList.Count; rank++)
                foreach (var alternative in referenceRankingList[rank])
                {
                    variantsList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
                    arternativesList.Add(alternative);
                }

            for (var i = 0; i < arternativesList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);

            otherAlternatives = notRankedAlternatives;
            otherAlternativesMatrix = CreateMatrix(otherAlternatives);

            transientMatrix = CreateMatrix(arternativesList);

            var cfc = 0;
            foreach (var partialUtility in partialUtilityList) cfc += partialUtility.Criterion.LinearSegments;
            criterionFieldsCount = cfc;

            arrayOfValues = new double[criterionFieldsCount];
            var count = 0;
            for (var numOfCriterion = 0; numOfCriterion < partialUtilityList.Count; numOfCriterion++)
            {
                var sum = 0f;
                for (var i = 1; i < partialUtilityList[numOfCriterion].PointsValues.Count; i++)
                {
                    var newValue = partialUtilityList[numOfCriterion].PointsValues[i].X - sum;
                    arrayOfValues[count] = newValue;
                    sum += newValue;
                    count++;
                }
            }


            var finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            var finalReferenceRanking = new FinalRanking(finalReferenceList);
            var recalculatedMatrix = RecreateMatrix(finalReferenceRanking);
            restrictionsMatrix = CalculateRestrictions(recalculatedMatrix, finalReferenceRanking);
            var tau = CalculateKendallCoefficient(finalReferenceRanking);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderByDescending(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.GetLength(0); i++) allFinalRankingEntry[i].Position = i + 1;

            Result.PartialUtilityFunctions = partialUtilityList;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        public void UpdatePreserveKendallCoefficient(bool preserveKendallCoefficient)
        {
            PreserveKendallCoefficient = preserveKendallCoefficient;
            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(restrictionsMatrix, arrayOfValues);
            var count = 0;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
            {
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[0].MinValue = 0;
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[0].MaxValue = 0;

                for (var i = 1; i < Result.PartialUtilityFunctions[numOfCriterion].PointsValues.Count; i++)
                {
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MinValue = (float) minArray[count];
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MaxValue = (float) maxArray[count];
                    count++;
                }
            }

            var finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            var finalReferenceRanking = new FinalRanking(finalReferenceList);
            var tau = CalculateKendallCoefficient(finalReferenceRanking);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderByDescending(o => o.Utility).ToList();
            for (var i = 0; i < allFinalRankingEntry.Count; i++) allFinalRankingEntry[i].Position = i + 1;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        public void ChangeValue(float value, PartialUtility partialUtility, int indexOfPointValue)
        {
            var recalculatedMatrix = RecreateMatrix(Result.FinalRanking);
            restrictionsMatrix = CalculateRestrictions(recalculatedMatrix, Result.FinalRanking);

            if (value < partialUtility.PointsValues[indexOfPointValue].MinValue ||
                value > partialUtility.PointsValues[indexOfPointValue].MaxValue)
                throw new ArgumentException("Value not in range", "PointsValues.Y");
            var count = 0;
            var criterionIndex = -1;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
                if (Result.PartialUtilityFunctions[numOfCriterion].Criterion.Name == partialUtility.Criterion.Name)
                {
                    criterionIndex = numOfCriterion;
                    break;
                }
                else
                {
                    count += Result.PartialUtilityFunctions[numOfCriterion].Criterion.LinearSegments;
                }

            if (indexOfPointValue < partialUtility.PointsValues.Count - 1)
            {
                arrayOfValues[count - 1 + indexOfPointValue + 1] += partialUtility.PointsValues[indexOfPointValue].Y - value;
                arrayOfValues[count - 1 + indexOfPointValue] -= partialUtility.PointsValues[indexOfPointValue].Y - value;
                partialUtility.PointsValues[indexOfPointValue].Y = value;
                Result.PartialUtilityFunctions[criterionIndex] = partialUtility;
            }
            else
            {
                var currentCount = 0;
                var subValue = (partialUtility.PointsValues[indexOfPointValue].Y - value) / (Result.PartialUtilityFunctions.Count - 1);
                for (var partialUtilityIndex = 0; partialUtilityIndex < Result.PartialUtilityFunctions.Count; partialUtilityIndex++)
                {
                    currentCount += Result.PartialUtilityFunctions[partialUtilityIndex].Criterion.LinearSegments;
                    if (partialUtilityIndex != criterionIndex)
                    {
                        var pointValue = Result.PartialUtilityFunctions[partialUtilityIndex].PointsValues;
                        pointValue[pointValue.Count - 1].Y += subValue;
                        arrayOfValues[currentCount - 1] += subValue;
                    }
                }

                partialUtility.PointsValues[partialUtility.PointsValues.Count - 1].Y = value;
                Result.PartialUtilityFunctions[criterionIndex] = partialUtility;
            }


            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(restrictionsMatrix, arrayOfValues);
            count = 0;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
            {
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[0].MinValue = 0;
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[0].MaxValue = 0;

                for (var i = 1; i < Result.PartialUtilityFunctions[numOfCriterion].PointsValues.Count; i++)
                {
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MinValue = (float) minArray[count];
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MaxValue = (float) maxArray[count];
                    count++;
                }
            }

            var finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            var finalReferenceRanking = new FinalRanking(finalReferenceList);
            var tau = CalculateKendallCoefficient(finalReferenceRanking);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderByDescending(o => o.Utility).ToList();
            for (var i = 0; i < allFinalRankingEntry.Count; i++) allFinalRankingEntry[i].Position = i + 1;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        private (double[] deepCopyofMin, double[] deepCopyofMax) CalculateRangeOfValues(double[,] restrictions, double[] arrayOfValues)
        {
            var min = new double[restrictions.GetLength(1) - 1];
            var max = new double[restrictions.GetLength(1) - 1];

            for (var i = 0; i < restrictions.GetLength(1) - 1; i++)
            {
                min[i] = 0;
                max[i] = 1;
            }

            var criterionEndPoints = new List<int>();
            var count = 0;
            for (var i = 0; i < criteriaList.Count; i++)
            {
                count += criteriaList[i].LinearSegments;
                criterionEndPoints.Add(count - 1);
            }


            for (var row = 0; row < restrictions.GetLength(0); row++)
            {
                count = 0;
                for (var numOfCriterion = 0; numOfCriterion < criteriaList.Count; numOfCriterion++)
                for (var i = 0; i < criteriaList[numOfCriterion].LinearSegments; i++)
                {
                    double localMin;
                    double localMax;
                    if (PreserveKendallCoefficient)
                    {
                        if (i == criteriaList[numOfCriterion].LinearSegments - 1)
                        {
                            (localMin, localMax) = CalculateValue(GetRow(restrictions, row), count, arrayOfValues, false);
                        }
                        else
                        {
                            (localMin, localMax) = CalculateValue(GetRow(restrictions, row), count, arrayOfValues, true);
                            if (localMax > arrayOfValues[count] + arrayOfValues[count + 1])
                                localMax = arrayOfValues[count] + arrayOfValues[count + 1];
                        }
                    }
                    else
                    {
                        if (i == criteriaList[numOfCriterion].LinearSegments - 1)
                        {
                            localMin = 0;
                            localMax = 1;
                        }
                        else
                        {
                            localMin = 0;
                            localMax = arrayOfValues[count] + arrayOfValues[count + 1];
                        }
                    }

                    if (localMin < 0) localMin = 0;
                    if (localMax > 1) localMax = 1;
                    if (min[count] < localMin) min[count] = localMin;
                    if (max[count] > localMax) max[count] = localMax;
                    count++;
                }
            }

            var deepCopyofMin = new double[min.Length];
            var deepCopyofMax = new double[max.Length];
            for (var i = 0; i < min.Length; i++)
            {
                deepCopyofMax[i] = max[i];
                deepCopyofMin[i] = min[i];
            }

            foreach (var element in criterionEndPoints)
            {
                double endPointsMin = 0;
                double endPointsMax = 1;
                foreach (var innerElement in criterionEndPoints)
                    if (element != innerElement)
                    {
                        if (endPointsMax > max[innerElement] - arrayOfValues[innerElement])
                            endPointsMax = max[innerElement] - arrayOfValues[innerElement];
                        if (endPointsMin > arrayOfValues[innerElement] - min[innerElement])
                            endPointsMin = arrayOfValues[innerElement] - min[innerElement];
                    }

                if (endPointsMax * (criterionEndPoints.Count - 1) < arrayOfValues[element] - min[element])
                    deepCopyofMin[element] = arrayOfValues[element] - endPointsMax * (criterionEndPoints.Count - 1);
                if (endPointsMin * (criterionEndPoints.Count - 1) < max[element] - arrayOfValues[element])
                    deepCopyofMax[element] = arrayOfValues[element] + endPointsMin * (criterionEndPoints.Count - 1);
            }

            count = 0;
            for (var numOfCriterion = 0; numOfCriterion < criteriaList.Count; numOfCriterion++)
            {
                var sum = (double) 0;
                for (var i = 0; i < criteriaList[numOfCriterion].LinearSegments; i++)
                {
                    deepCopyofMin[count] += sum;
                    deepCopyofMax[count] += sum;
                    sum += arrayOfValues[count];
                    if (deepCopyofMin[count] > 1) deepCopyofMin[count] = 1;
                    if (deepCopyofMax[count] > 1) deepCopyofMax[count] = 1;
                    count++;
                }
            }


            return (deepCopyofMin, deepCopyofMax);
        }


        private (double min, double max) CalculateValue(double[] row, int index, double[] arrayOfValues, bool hasNext)
        {
            var min = (double) -1;
            var max = (double) 1;
            var nominator = (double) 0;
            var denominator = (double) 0;
            var sum = (double) 0;
            var precision = 10000000;

            for (var i = 0; i < arrayOfValues.Length; i++)
                nominator -= arrayOfValues[i] * row[i];
            nominator += row[row.Length - 1];


            if (hasNext)
            {
                if (Math.Abs(row[index] - row[index + 1]) < EpsilonThreshold && Math.Abs(nominator) >= EpsilonThreshold && nominator > 0)
                    throw new ArgumentException("Error equation", "nominator");
                if (Math.Abs(row[index]) < EpsilonThreshold)
                    denominator = -row[index + 1];
                else if (Math.Abs(row[index + 1]) < EpsilonThreshold)
                    denominator = row[index];
                else
                    denominator = row[index] - row[index + 1];
            }
            else
            {
                denominator = row[index];
            }

            if (hasNext)
            {
                if (Math.Abs(row[index]) < EpsilonThreshold && Math.Abs(row[index + 1]) < EpsilonThreshold) return (-1, 1);
            }
            else
            {
                if (Math.Abs(row[index]) < EpsilonThreshold) return (-1, 1);
            }

            if (row[row.Length - 1] > 0)
            {
                if (Math.Abs(nominator) < EpsilonThreshold)
                {
                    if (denominator > 0 && Math.Abs(denominator) > EpsilonThreshold)
                    {
                        sum = Math.Ceiling(arrayOfValues[index] * precision) / precision;
                        min = sum;
                        max = 1;
                    }
                    else if (denominator < 0 && Math.Abs(denominator) > EpsilonThreshold)
                    {
                        sum = Math.Floor(arrayOfValues[index] * precision) / precision;
                        min = -1;
                        max = sum;
                    }
                    else
                    {
                        min = -1;
                        max = 1;
                    }

                    return (min, max);
                }

                sum = arrayOfValues[index] + nominator / denominator;
                if (Math.Abs(denominator) < EpsilonThreshold)
                {
                    min = -1;
                    max = 1;
                }
                else if (denominator > 0)
                {
                    sum = Math.Ceiling(sum * precision) / precision;
                    min = sum;
                    max = 1;
                }
                else
                {
                    sum = Math.Floor(sum * precision) / precision;
                    min = -1;
                    max = sum;
                }

                return (min, max);
            }

            if (Math.Abs(nominator) < EpsilonThreshold)
            {
                if (Math.Abs(denominator) > EpsilonThreshold)
                {
                    min = arrayOfValues[index];
                    max = arrayOfValues[index];
                }
                else
                {
                    min = -1;
                    max = 1;
                }

                return (min, max);
            }

            sum = arrayOfValues[index] + nominator / denominator;
            if (Math.Abs(denominator) < EpsilonThreshold)
            {
                min = -1;
                max = 1;
            }
            else if (denominator > 0)
            {
                min = sum;
                max = sum;
            }
            else
            {
                min = sum;
                max = sum;
            }

            return (min, max);
        }

        private double[,] CalculateRestrictions(double[,] recalculatedMatrix, FinalRanking finalRanking)
        {
            var matrix = new double[variantsList.Count - 1, criterionFieldsCount + 1];

            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < criterionFieldsCount; c++) matrix[r, c] = recalculatedMatrix[r, c] - recalculatedMatrix[r + 1, c];
                if (Math.Round(finalRanking.FinalRankingCollection[r].Utility - finalRanking.FinalRankingCollection[r + 1].Utility, 14) >=
                    DeltaThreshold)
                    matrix[r, matrix.GetLength(1) - 1] = DeltaThreshold;
                else
                    matrix[r, matrix.GetLength(1) - 1] = 0;
            }

            return matrix;
        }

        public double[,] RecreateMatrix(FinalRanking finalRanking)
        {
            var finalRankingEntryList = finalRanking.FinalRankingCollection;
            var matrix = new double[finalRankingEntryList.Count, criterionFieldsCount];
            for (var i = 0; i < variantsList.Count; i++)
            {
                var row = GenerateRow(criterionFieldsCount, finalRankingEntryList[i].Alternative);
                for (var j = 0; j < criterionFieldsCount; j++) matrix[i, j] = row[j];
            }

            return matrix;
        }

        private float CalculateKendallCoefficient(FinalRanking finalReferenceRanking)
        {
            var matrix1 = CreateKendallMatrix();
            var matrix2 = CreateKendallMatrix(finalReferenceRanking);
            float lengthBetweenMatrix = 0, tau;
            for (var row = 0; row < matrix1.GetLength(0); row++)
            for (var c = 0; c < matrix1.GetLength(1); c++)
                lengthBetweenMatrix += Math.Abs(matrix1[row, c] - matrix2[row, c]);

            lengthBetweenMatrix /= 2;
            tau = 1 - 4 * lengthBetweenMatrix /
                  (finalReferenceRanking.FinalRankingCollection.Count * (finalReferenceRanking.FinalRankingCollection.Count - 1));
            return tau;
        }

        private float[,] CreateKendallMatrix()
        {
            var rankingMatrix = new float[variantsList.Count, variantsList.Count];
            for (var row = 0; row < rankingMatrix.GetLength(0); row++)
            for (var c = 0; c < rankingMatrix.GetLength(1); c++)
            {
                if (row == c || variantsList[c].Value - variantsList[row].Value > 0)
                    rankingMatrix[row, c] = 0;
                if (variantsList[c].Value - variantsList[row].Value == 0)
                    rankingMatrix[row, c] = 0.5F;
                rankingMatrix[row, c] = 1;
            }

            return rankingMatrix;
        }

        private float[,] CreateKendallMatrix(FinalRanking ranking)
        {
            var rankingMatrix = new float[ranking.FinalRankingCollection.Count, ranking.FinalRankingCollection.Count];
            for (var row = 0; row < rankingMatrix.GetLength(0); row++)
            for (var c = 0; c < rankingMatrix.GetLength(1); c++)
            {
                if (row == c || ranking.FinalRankingCollection[c].Utility - ranking.FinalRankingCollection[row].Utility > DeltaThreshold)
                    rankingMatrix[row, c] = 0;
                if (ranking.FinalRankingCollection[c].Utility - ranking.FinalRankingCollection[row].Utility <= DeltaThreshold)
                    rankingMatrix[row, c] = 0.5F;
                rankingMatrix[row, c] = 1;
            }

            return rankingMatrix;
        }

        private ObservableCollection<FinalRankingEntry> CreateRanking(double[] arrayOfValues, double[,] transientMatrix,
            List<Alternative> listOfAlternatives)
        {
            var finalRankingList = new List<FinalRankingEntry>();
            for (var i = 0; i < transientMatrix.GetLength(0); i++)
            {
                var score = CreateFinalRankingEntryUtility(arrayOfValues, GetRow(transientMatrix, i));
                var finalRankingEntry = new FinalRankingEntry(-1, listOfAlternatives[i], (float) score);
                finalRankingList.Add(finalRankingEntry);
            }

            var finalRankingSorted = finalRankingList.OrderByDescending(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.GetLength(0); i++) finalRankingSorted[i].Position = i;

            return new ObservableCollection<FinalRankingEntry>(finalRankingSorted);
        }

        private double CreateFinalRankingEntryUtility(double[] arrayOfValues, double[] row)
        {
            double score = 0;
            for (var i = 0; i < arrayOfValues.Length; i++) score += arrayOfValues[i] * row[i];
            return score;
        }

        private (List<PartialUtility>, double[]) MakePartialUtilityFunction(Dictionary<double, double> doubles)
        {
            var partialUtilityList = new List<PartialUtility>();
            List<PartialUtilityValues> list;
            double[] partialArray = { }, arrayOfValues = { };
            var count = 0;
            foreach (var criterion in criteriaList)
            {
                (count, list, partialArray) = NextPartialUtility(doubles, criterion, count);
                arrayOfValues = AddTailToArray(arrayOfValues, partialArray);
                partialUtilityList.Add(new PartialUtility(criterion, list));
            }

            return (partialUtilityList, arrayOfValues);
        }

        private double[] AddTailToArray(double[] arrayOfValues, double[] partialArray)
        {
            var newArray = new double[arrayOfValues.Length + partialArray.Length];
            for (var i = 0; i < arrayOfValues.Length; i++) newArray[i] = arrayOfValues[i];
            for (var i = arrayOfValues.Length; i < arrayOfValues.Length + partialArray.Length; i++)
                newArray[i] = partialArray[i - arrayOfValues.Length];

            return newArray;
        }

        private (int, List<PartialUtilityValues>, double[]) NextPartialUtility(Dictionary<double, double> doubles, Criterion criterion,
            int count)
        {
            var linearSegments = criterion.LinearSegments;
            var arrayOfValues = new double[criterion.LinearSegments];
            List<PartialUtilityValues> list;
            double segmentValue = (criterion.MaxValue - criterion.MinValue) / linearSegments, currentPoint, currentValue = 0;
            if (criterion.CriterionDirection == "Gain")
            {
                currentPoint = criterion.MinValue;
                var partialUtilityValues = new PartialUtilityValues((float) currentPoint, (float) currentValue);
                list = new List<PartialUtilityValues> {partialUtilityValues};
                for (var s = 0; s < linearSegments; s++)
                {
                    currentPoint = criterion.MinValue + (s + 1) * segmentValue;
                    arrayOfValues[s] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s] = doubles[count];
                    }

                    partialUtilityValues = new PartialUtilityValues((float) currentPoint, (float) currentValue);
                    list.Add(partialUtilityValues);
                    count++;
                }
            }
            else
            {
                currentPoint = criterion.MaxValue;
                var partialUtilityValues = new PartialUtilityValues((float) currentPoint, (float) currentValue);
                list = new List<PartialUtilityValues> {partialUtilityValues};
                for (var s = 0; s < linearSegments; s++)
                {
                    currentPoint = criterion.MaxValue - (s + 1) * segmentValue;
                    arrayOfValues[s] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s] = doubles[count];
                    }

                    partialUtilityValues = new PartialUtilityValues((float) currentPoint, (float) currentValue);
                    list.Add(partialUtilityValues);
                    count++;
                }
            }

            return (count, list, arrayOfValues);
        }


        public double[,] CreateSimplexMatrix(int height, int width, double[,] matrix)
        {
            var simplexMatrix = new double[height, width];
            for (var r = 0; r < height; r++)
            for (var c = 0; c < height; c++)
                simplexMatrix[r, c] = 0;

            var widthWithoutSlack = width - height - variantsList.Count + equals.Count;
            var cj = new double[width];
            var basic = new int[height];
            for (var c = 0; c < cj.Length; c++) cj[c] = 0;

            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < criterionFieldsCount; c++) simplexMatrix[r, c] = matrix[r, c] - matrix[r + 1, c];
                simplexMatrix[r, r * 2 + criterionFieldsCount] = -1;
                simplexMatrix[r, r * 2 + criterionFieldsCount + 1] = 1;
                simplexMatrix[r, r * 2 + criterionFieldsCount + 2] = 1;
                simplexMatrix[r, r * 2 + criterionFieldsCount + 3] = -1;
                for (var field = 0; field < 4; field++)
                    cj[r * 2 + criterionFieldsCount + field] = 1;
            }

            for (var c = 0; c < criterionFieldsCount; c++)
                simplexMatrix[variantsList.Count - 1, c] = c < criterionFieldsCount ? 1 : 0;

            var i = 0;
            var bound = widthWithoutSlack + variantsList.Count - equals.Count;
            for (var r = 0; r < height; r++)
            {
                for (var c = widthWithoutSlack; c < bound; c++)
                    if (!equals.Contains(r) && c == widthWithoutSlack + i)
                    {
                        simplexMatrix[r, c] = -1;
                        i++;
                        break;
                    }

                for (var c = bound; c < width; c++)
                    simplexMatrix[r, c] = c != r + bound ? 0 : 1;
            }

            for (var c = bound; c < width; c++)
                cj[c] = double.PositiveInfinity;

            for (var r = 0; r < height; r++)
                basic[r] = bound + r;

            vectorCj = cj;
            basicVariables = basic;
            simplexMatrix = AddAnswerColumn(simplexMatrix);
            return simplexMatrix;
        }

        public double[,] AddAnswerColumn(double[,] sM)
        {
            int height = sM.GetLength(0), width = sM.GetLength(1) + 1;
            var simplexMatrix = new double[height, width];
            for (var row = 0; row < height; row++)
            {
                for (var c = 0; c < sM.GetLength(1); c++) simplexMatrix[row, c] = sM[row, c];
                simplexMatrix[row, simplexMatrix.GetLength(1) - 1] = equals.Contains(row) ? 0 : DeltaThreshold;
            }

            simplexMatrix[height - 1, simplexMatrix.GetLength(1) - 1] = 1;

            return simplexMatrix;
        }

        public double[,] CreateMatrix(List<Alternative> listOfAlternatives)
        {
            var matrix = new double[listOfAlternatives.Count, criterionFieldsCount];
            for (var i = 0; i < listOfAlternatives.Count; i++)
            {
                var row = GenerateRow(criterionFieldsCount, listOfAlternatives[i]);
                for (var j = 0; j < criterionFieldsCount; j++) matrix[i, j] = row[j];
            }

            return matrix;
        }

        public double[] GenerateRow(int width, Alternative ae)
        {
            var row = new double[width];
            var index = 0;
            foreach (var entry in ae.CriteriaValuesList)
            {
                var tmpCriterion = new Criterion();
                foreach (var criterion in criteriaList)
                    if (criterion.Name == entry.Name)
                        tmpCriterion = criterion;
                var fields = GenerateCriterionFields(tmpCriterion, (float) entry.Value); //TODO cast z float? na float
                for (var j = 0; j < fields.Length; j++) row[index++] = fields[j];
            }

            return row;
        }

        public double[] GenerateCriterionFields(Criterion criterion, float value)
        {
            var linearSegments = criterion.LinearSegments;
            double segmentValue = (criterion.MaxValue - criterion.MinValue) / linearSegments, lowerBound, upperBound;
            var fields = new double[linearSegments];
            if (criterion.CriterionDirection == "Gain")
            {
                lowerBound = criterion.MinValue;
                for (var s = 0; s < linearSegments; s++)
                {
                    upperBound = criterion.MinValue + (s + 1) * segmentValue;
                    if (value < upperBound)
                    {
                        if (value <= lowerBound)
                        {
                            fields[s] = 0;
                        }
                        else
                        {
                            fields[s] = Math.Round((value - lowerBound) / (upperBound - lowerBound), 14);
                            if (s > 0) fields[s - 1] = 1;
                        }
                    }
                    else
                    {
                        fields[s] = 1;
                    }

                    lowerBound = upperBound;
                }
            }
            else
            {
                lowerBound = criterion.MaxValue;
                for (var s = 0; s < linearSegments; s++)
                {
                    upperBound = criterion.MaxValue - (s + 1) * segmentValue;
                    if (value > upperBound)
                    {
                        if (value >= lowerBound)
                        {
                            fields[s] = 0;
                        }
                        else
                        {
                            fields[s] = Math.Round((lowerBound - value) / (lowerBound - upperBound), 14);
                            if (s > 0) fields[s - 1] = 1;
                        }
                    }
                    else
                    {
                        fields[s] = 1;
                    }

                    lowerBound = upperBound;
                }
            }

            return fields;
        }

        public int WidthOfSimplexMatrix(Alternative alternative)
        {
            var width = criterionFieldsCount;
            width += variantsList.Count * 2;
            width += variantsList.Count + variantsList.Count - equals.Count;
            return width;
        }

        public double[] GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
}