using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;

namespace CalculationsEngine
{
    internal class Solver
    {
        private readonly List<Alternative> arternativesList;
        private readonly List<int> equals;
        private readonly List<Alternative> otherAlternatives;
        private readonly double[,] otherAlternativesMatrix;
        private readonly ReferenceRanking referenceRanking;
        private readonly List<KeyValuePair<Alternative, int>> variantsList;
        private double[] arrayOfValues;
        private Dictionary<double, double> solution;
        private double[,] transientMatrix;


        public Solver(ReferenceRanking referenceRanking, List<Alternative> otherAlternatives, Results results)
        {
            Result = results;
            this.referenceRanking = referenceRanking;
            variantsList = new List<KeyValuePair<Alternative, int>>();
            arternativesList = new List<Alternative>();
            for(var rank = 0; rank < referenceRanking.RankingsCollection.Count; rank++)
                foreach (var alternative in referenceRanking.RankingsCollection[rank])
                {
                    variantsList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
                    arternativesList.Add(alternative);
                }

            var cfc = 0;
            foreach (var criterion in variantsList[0].Key.CriteriaValues.Keys) cfc += criterion.LinearSegments;
            CriterionFieldsCount = cfc;
            equals = new List<int>();
            for (var i = 0; i < variantsList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);
            this.otherAlternatives = otherAlternatives;
            otherAlternativesMatrix = CreateMatrix(otherAlternatives);
        }

        private int CriterionFieldsCount { get; }
        private double[,] Matrix { get; set; }
        public Results Result { get; set; }
        private int[] BasicVariables { get; set; }
        private double[,] RestrictionsMatrix { get; set; }
        private double[] Cj { get; set; }
        private int NumberOfIteration { get; set; }

        public void Calculate()
        {
            var heightOfPrimaryMatrix = variantsList[0].Key.CriteriaValues.Count;
            var height = HeightOfSimplexMatrix();
            var width = WidthOfSimplexMatrix(variantsList[0].Key);
            List<PartialUtility> partialUtilityList;


            transientMatrix = CreateMatrix(arternativesList);
            Matrix = CreateSimplexMatrix(height, width, transientMatrix);

            var simplex =
                new Simplex(Matrix, BasicVariables, Cj);
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
                RestrictionsMatrix = CalculateRestrictions(recalculatedMatrix, finalReferenceRanking);
            }
            else
            {
                RestrictionsMatrix = CalculateRestrictions(Matrix, finalReferenceRanking);
            }

            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(RestrictionsMatrix, variantsList[0].Key, arrayOfValues);
            var count = 0;
            for (var numOfCriterion = 0; numOfCriterion < partialUtilityList.Count; numOfCriterion++)
            for (var i = 0; i < partialUtilityList[numOfCriterion].PointsValues.Count; i++)
            {
                partialUtilityList[numOfCriterion].PointsValues[i].MinValue = (float) minArray[count];
                partialUtilityList[numOfCriterion].PointsValues[i].MaxValue = (float) maxArray[count];
                count++;
            }

            var tau = CalculateKendallCoefficient(finalReferenceRanking);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++) allFinalRankingEntry[i].Position = i;

            Result.PartialUtilityFunctions = partialUtilityList;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        public void ChangeValue(float value, PartialUtility partialUtility, float point)
        {
            var count = 0;
            var index = -1;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
                if (Result.PartialUtilityFunctions[numOfCriterion].Criterion.Equals(partialUtility.Criterion))
                {
                    index = numOfCriterion;
                    break;
                }
                else
                {
                    count += Result.PartialUtilityFunctions[numOfCriterion].Criterion.LinearSegments;
                }

            for (var i = 0; i < partialUtility.PointsValues.Count; i++)
                if (partialUtility.PointsValues[i].X == point)
                {
                    if (i < partialUtility.PointsValues.Count - 1)
                    {
                        arrayOfValues[count - 1 + i + 1] += partialUtility.PointsValues[i].Y - value;
                        arrayOfValues[count - 1 + i] -= partialUtility.PointsValues[i].Y - value;
                        partialUtility.PointsValues[i].Y = value;
                        Result.PartialUtilityFunctions[index] = partialUtility;
                    }
                    else
                    {
                        var currentCount = 0;
                        var subValue = (partialUtility.PointsValues[i].Y - value) / (Result.PartialUtilityFunctions.Count - 1);
                        for (var partialUtilityIndex = 0; i < Result.PartialUtilityFunctions.Count; i++)
                        {
                            currentCount += Result.PartialUtilityFunctions[partialUtilityIndex].Criterion.LinearSegments;
                            if (partialUtilityIndex != index)
                            {
                                var pointValue = Result.PartialUtilityFunctions[partialUtilityIndex].PointsValues;
                                pointValue[pointValue.Count - 1].Y += subValue;
                                arrayOfValues[currentCount - 1] += subValue;
                            }
                        }

                        partialUtility.PointsValues[partialUtility.PointsValues.Count - 1].Y = value;
                        Result.PartialUtilityFunctions[index] = partialUtility;
                    }
                }

            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(RestrictionsMatrix, variantsList[0].Key, arrayOfValues);
            count = 0;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
            for (var i = 0; i < Result.PartialUtilityFunctions[numOfCriterion].PointsValues.Count; i++)
            {
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MinValue = (float) minArray[count];
                Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MaxValue = (float) maxArray[count];
                count++;
            }

            var finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            var restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++) allFinalRankingEntry[i].Position = i;
            Result.FinalRanking.FinalRankingCollection = new ObservableCollection<FinalRankingEntry>(allFinalRankingEntry);
        }

        private (double[] deepCopyofMin, double[] deepCopyofMax) CalculateRangeOfValues(double[,] restrictionsMatrix,
            Alternative exampleAlternative, double[] arrayOfValues)
        {
            var min = new double[restrictionsMatrix.GetLength(1) - 1];
            var max = new double[restrictionsMatrix.GetLength(1) - 1];

            for (var i = 0; i < restrictionsMatrix.GetLength(1) - 1; i++)
            {
                min[i] = 0;
                max[i] = 1;
            }

            var arrayOfCriterion = exampleAlternative.CriteriaValues.Keys.ToArray();
            var criterionEndPoints = new List<int>();
            var count = 0;
            for (var i = 0; i < arrayOfCriterion.Length; i++)
            {
                count += arrayOfCriterion[i].LinearSegments;
                criterionEndPoints.Add(count);
            }


            for (var row = 0; row < restrictionsMatrix.GetLength(0); row++)
            {
                count = 0;
                for (var numOfCriterion = 0; numOfCriterion < arrayOfCriterion.Length; numOfCriterion++)
                for (var i = 0; i < arrayOfCriterion[numOfCriterion].LinearSegments; i++)
                {
                    double localMin;
                    double localMax;
                    if (i == arrayOfCriterion[numOfCriterion].LinearSegments - 1)
                        (localMin, localMax) = CalculateValue(GetRow(restrictionsMatrix, row), count, arrayOfValues, false);
                    else
                        (localMin, localMax) = CalculateValue(GetRow(restrictionsMatrix, row), count, arrayOfValues, true);
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
                        if (endPointsMin < arrayOfValues[innerElement] - min[innerElement])
                            endPointsMin = arrayOfValues[innerElement] - min[innerElement];
                    }

                if (endPointsMax * (criterionEndPoints.Count - 1) < arrayOfValues[element] - min[element])
                    deepCopyofMin[element] = endPointsMax * (criterionEndPoints.Count - 1);
                if (endPointsMin * (criterionEndPoints.Count - 1) > max[element] - arrayOfValues[element])
                    deepCopyofMin[element] = endPointsMax * (criterionEndPoints.Count - 1);
            }

            return (deepCopyofMin, deepCopyofMax);
        }

        private (double min, double max) CalculateValue(double[] row, int index, double[] arrayOfValues, bool hasNext)
        {
            var min = (double) 0;
            var max = (double) 1;
            var sum = (double) 0;


            if (row[index] == 0) return (min, max);

            if (row[index] > 0)
            {
                for (var i = 0; i < row.Length - 1; i++)
                    if (i != index)
                        sum -= arrayOfValues[i] * row[i];
                sum += arrayOfValues[row.Length - 1];
                sum /= row[index];
                if (hasNext)
                    if (max > row[index] + row[index + 1])
                        max = row[index] + row[index + 1];
                return (sum, max);
            }

            for (var i = 0; i < row.Length - 1; i++)
                if (i != index)
                    sum -= arrayOfValues[i] * row[i];
            sum += arrayOfValues[row.Length - 1];
            sum /= -row[index];
            if (hasNext)
                if (sum > row[index] + row[index + 1])
                    sum = row[index] + row[index + 1];
            return (min, sum);
        }


        private double[,] CalculateRestrictions(double[,] recalculatedMatrix, FinalRanking finalRanking)
        {
            var matrix = new double[recalculatedMatrix.GetLength(0) - 1, recalculatedMatrix.GetLength(1) + 1];

            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < CriterionFieldsCount; c++) matrix[r, c] = recalculatedMatrix[r, c] - recalculatedMatrix[r + 1, c];
                if (finalRanking.FinalRankingCollection[r + 1].Utility - finalRanking.FinalRankingCollection[r].Utility > 0.05)
                    matrix[r, matrix.GetLength(1)] = 0;
                else
                    matrix[r, matrix.GetLength(1)] = 0.05;
            }

            return matrix;
        }

        public double[,] RecreateMatrix(FinalRanking finalRanking)
        {
            var finalRankingEntryList = finalRanking.FinalRankingCollection;
            var matrix = new double[finalRankingEntryList[0].Alternative.CriteriaValues.Count, CriterionFieldsCount];
            for (var i = 0; i < variantsList.Count; i++)
            {
                var row = GenerateRow(CriterionFieldsCount, finalRankingEntryList[i].Alternative);
                for (var j = 0; j < CriterionFieldsCount; j++) matrix[i, j] = row[j];
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
                if (row == c || ranking.FinalRankingCollection[c].Utility - ranking.FinalRankingCollection[row].Utility > 0.05)
                    rankingMatrix[row, c] = 0;
                if (ranking.FinalRankingCollection[c].Utility - ranking.FinalRankingCollection[row].Utility <= 0.05) rankingMatrix[row, c] = 0.5F;
                rankingMatrix[row, c] = 1;
            }

            return rankingMatrix;
        }

        private ObservableCollection<FinalRankingEntry> CreateRanking(double[] arrayOfValues, double[,] transientMatrix,
            List<Alternative> listOfAlternatives)
        {
            var finalRankingList = new List<FinalRankingEntry>();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                var score = CreateFinalRankingEntryUtility(arrayOfValues, GetRow(transientMatrix, i));
                var finalRankingEntry = new FinalRankingEntry(-1, listOfAlternatives[i], (float) score);
                finalRankingList.Add(finalRankingEntry);
            }

            var finalRankingSorted = finalRankingList.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++) finalRankingSorted[i].Position = i;

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
            foreach (var entry in variantsList[0].Key.CriteriaValues)
            {
                (count, list, partialArray) = NextPartialUtility(doubles, entry.Key, count);
                arrayOfValues = AddTailToArray(arrayOfValues, partialArray);
                partialUtilityList.Add(new PartialUtility(entry.Key, list));
                //TODO min max - range
            }

            return (partialUtilityList, arrayOfValues);
        }

        private double[] AddTailToArray(double[] arrayOfValues, double[] partialArray)
        {
            var newArray = new double[arrayOfValues.Length + partialArray.Length];
            for (var i = 0; i < arrayOfValues.Length; i++) newArray[i] = arrayOfValues[i];
            for (var i = arrayOfValues.Length; i < arrayOfValues.Length + partialArray.Length; i++) newArray[i] = partialArray[i];

            return newArray;
        }

        private (int, List<PartialUtilityValues>, double[]) NextPartialUtility(Dictionary<double, double> doubles, Criterion criterion,
            int count)
        {
            var linearSegments = criterion.LinearSegments;
            var arrayOfValues = new double[criterion.LinearSegments];
            List<PartialUtilityValues> list;
            double segmentValue = (criterion.MaxValue - criterion.MinValue) / linearSegments, currentPoint, currentValue = 0;
            if (criterion.CriterionDirection == "g")
            {
                currentPoint = criterion.MinValue;
                var partialUtilityValues = new PartialUtilityValues((float) currentPoint, (float) currentValue);
                list = new List<PartialUtilityValues> {partialUtilityValues};
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MinValue + s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
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
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MaxValue - s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
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
            var widthWithoutSlack = width - height - equals.Count;
            var cj = new double[width];
            var basic = new int[height];
            for (var c = 0; c < cj.Length; c++) cj[c] = 0;

            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < CriterionFieldsCount; c++) simplexMatrix[r, c] = matrix[r, c] - matrix[r + 1, c];
                simplexMatrix[r, r * 2 + CriterionFieldsCount] = -1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 1] = 1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 2] = 1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 3] = -1;
                for (var field = 0; field < 4; field++)
                    cj[r * 2 + CriterionFieldsCount + field] = 1;
            }

            //Create last ideal line = 1
            for (var c = 0; c < CriterionFieldsCount; c++)
                simplexMatrix[variantsList.Count - 1, c] = c < CriterionFieldsCount ? 1 : 0;
            //Add slack 1 and subtract surplus -1 variables.
            var i = 0;
            var bound = widthWithoutSlack + height - equals.Count - 1;
            for (var r = 0; r < height; r++)
            {
                for (var c = widthWithoutSlack; c < bound; c++)
                    if (equals.Contains(r) && c == widthWithoutSlack + i)
                    {
                        simplexMatrix[r, c] = equals.Contains(r) ? -1 : 0;
                        i++;
                    }

                for (var c = bound; c < width; c++)
                    simplexMatrix[r, c] = c != r + widthWithoutSlack ? 0 : 1;
            }

            for (var c = widthWithoutSlack; c < width; c++)
                if (c < bound)
                    cj[c] = c < bound ? 0 : double.PositiveInfinity;

            for (var r = 0; r < height; r++)
                basic[r] = bound + r;

            Cj = cj;
            BasicVariables = basic;
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
                simplexMatrix[row, sM.GetLength(1)] = equals.Contains(row) ? 0.05 : 0;
            }

            simplexMatrix[height, sM.GetLength(1)] = 1;

            return simplexMatrix;
        }

        public double[,] CreateMatrix(List<Alternative> listOfAlternatives)
        {
            var matrix = new double[listOfAlternatives[0].CriteriaValues.Count, CriterionFieldsCount];
            for (var i = 0; i < variantsList.Count; i++)
            {
                var row = GenerateRow(CriterionFieldsCount, listOfAlternatives[i]);
                for (var j = 0; j < CriterionFieldsCount; j++) matrix[i, j] = row[j];
            }

            return matrix;
        }

        public double[] GenerateRow(int width, Alternative ae)
        {
            var row = new double[width];
            var index = 0;
            foreach (var entry in ae.CriteriaValues)
            {
                var fields = GenerateCriterionFields(entry);
                for (var j = 0; j < fields.Length; j++) row[index++] = j;
            }

            return row;
        }

        public double[] GenerateCriterionFields(KeyValuePair<Criterion, float> av)
        {
            var linearSegments = av.Key.LinearSegments;
            double segmentValue = (av.Key.MaxValue - av.Key.MinValue) / linearSegments, lowerBound, upperBound;
            var fields = new double[linearSegments];
            if (av.Key.CriterionDirection == "g")
            {
                lowerBound = av.Key.MinValue;
                for (var s = 0; s < linearSegments; s++)
                {
                    upperBound = av.Key.MinValue + (s + 1) * segmentValue;
                    if (av.Value < upperBound)
                    {
                        if (av.Value <= lowerBound)
                        {
                            fields[s] = 0;
                        }
                        else
                        {
                            fields[s] = (av.Value - lowerBound) / (upperBound - lowerBound);
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
                lowerBound = av.Key.MaxValue;
                for (var s = 0; s < linearSegments; s++)
                {
                    upperBound = av.Key.MaxValue - (s + 1) * segmentValue;
                    if (av.Value > upperBound)
                    {
                        if (av.Value >= lowerBound)
                        {
                            fields[s] = 0;
                        }
                        else
                        {
                            fields[s] = (lowerBound - av.Value) / (lowerBound - upperBound);
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
            var width = CriterionFieldsCount;
            width += (alternative.CriteriaValues.Count - 1) * 2 + 4;
            width += HeightOfSimplexMatrix() + equals.Count;
            return width;
        }

        public int HeightOfSimplexMatrix()
        {
            return variantsList.Count;
        }

        public double[] GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
}