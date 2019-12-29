using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DataModel.Structs;

namespace CalculationsEngine
{
    internal class Solver
    {
        private readonly List<int> equals;
        private readonly ReferenceRanking referenceRanking;
        private readonly ReferenceRanking allVariantsRanking;
        private readonly List<KeyValuePair<Alternative, int>> variantsList;
        private readonly List<Alternative> arternativesList;
        private readonly List<Alternative> otherAlternatives;
        private double[] arrayOfValues;
        private Dictionary<double, double> solution;
        private double[,] transientMatrix;
        private double[,] otherAlternativesMatrix;


        public Solver(ReferenceRanking referenceRanking, List<Alternative> otherAlternatives)
        {
            this.referenceRanking = referenceRanking;
            variantsList = new List<KeyValuePair<Alternative, int>>();
            arternativesList = new List<Alternative>();
            foreach (var referenceRankingEntry in referenceRanking.ReferenceRankingList)
            {
                variantsList.Add(new KeyValuePair<Alternative, int>(referenceRankingEntry.Alternative,
                                    referenceRankingEntry.Rank));
                arternativesList.Add(referenceRankingEntry.Alternative);
            }
                
            //variantsList = referenceRanking.AlternativeList;
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
            float tau;

            var simplex =
                new Simplex(Matrix, BasicVariables, Cj); //widthOfPrimary = width - height, widthOfSlack = height
            simplex.Run(NumberOfIteration);
            solution = simplex.solution;
            (partialUtilityList, arrayOfValues) = MakePartialUtilityFunction(solution);
            List<FinalRankingEntry> finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            FinalRanking finalReferenceRanking = new FinalRanking(finalReferenceList);
            bool recalculate = false;
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
            {
                for (int i = 0; i < partialUtilityList[numOfCriterion].PointsValues.Count; i++)
                {
                    partialUtilityList[numOfCriterion].PointsValues[i].MinValue = (float)minArray[count];
                    partialUtilityList[numOfCriterion].PointsValues[i].MaxValue = (float)maxArray[count];
                    count++;
                }
            }

            tau = CalculateKendallCoefficient(finalReferenceRanking);
            List<FinalRankingEntry> restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                allFinalRankingEntry[i].Position = i;
            }

            Result.PartialUtilityFunctions = partialUtilityList;
            Result.FinalRanking = new FinalRanking(allFinalRankingEntry);
            Result.KendallCoefficient = tau;
        }

        public void ChangeValue(float value, PartialUtility partialUtility, float point)
        {
            var count = 0;
            var index = -1;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
            {
                if (Result.PartialUtilityFunctions[numOfCriterion].Criterion.Equals(partialUtility.Criterion))
                {
                    index = numOfCriterion;
                    break;
                }
                else
                {
                    count += Result.PartialUtilityFunctions[numOfCriterion].Criterion.LinearSegments;
                }
            }
            for (int i = 0; i < partialUtility.PointsValues.Count; i++)
                if (partialUtility.PointsValues[i].Point == point)
                {
                    if (i < partialUtility.PointsValues.Count - 1)
                    {
                        //partialUtility.PointsValues[i + 1].Value -= partialUtility.PointsValues[i].Value - value;
                        
                        arrayOfValues[count - 1 + i + 1] += partialUtility.PointsValues[i].Value - value;
                        arrayOfValues[count - 1 + i] -= partialUtility.PointsValues[i].Value - value;
                        partialUtility.PointsValues[i].Value = value;
                        Result.PartialUtilityFunctions[index] = partialUtility;
                    }
                    else
                    {
                        var currentCount = 0;
                        var subValue = (partialUtility.PointsValues[i].Value - value) / (Result.PartialUtilityFunctions.Count - 1);
                        for (var partialUtilityIndex = 0; i < Result.PartialUtilityFunctions.Count; i++)
                        {
                            currentCount += Result.PartialUtilityFunctions[partialUtilityIndex].Criterion.LinearSegments;
                            if (partialUtilityIndex != index)
                            {
                                var pointValue = Result.PartialUtilityFunctions[partialUtilityIndex].PointsValues;
                                pointValue[pointValue.Count - 1].Value += subValue;
                                arrayOfValues[currentCount - 1] += subValue;
                            }
                        }
                        partialUtility.PointsValues[partialUtility.PointsValues.Count - 1].Value = value;
                        Result.PartialUtilityFunctions[index] = partialUtility;
                    }
                }

            double[] minArray;
            double[] maxArray;
            (minArray, maxArray) = CalculateRangeOfValues(RestrictionsMatrix, variantsList[0].Key, arrayOfValues);
            count = 0;
            for (var numOfCriterion = 0; numOfCriterion < Result.PartialUtilityFunctions.Count; numOfCriterion++)
            {
                for (int i = 0; i < Result.PartialUtilityFunctions[numOfCriterion].PointsValues.Count; i++)
                {
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MinValue = (float)minArray[count];
                    Result.PartialUtilityFunctions[numOfCriterion].PointsValues[i].MaxValue = (float)maxArray[count];
                    count++;
                }
            }

            List<FinalRankingEntry> finalReferenceList = CreateRanking(arrayOfValues, transientMatrix, arternativesList);
            List<FinalRankingEntry> restOfReferenceList = CreateRanking(arrayOfValues, otherAlternativesMatrix, otherAlternatives);
            var allFinalRankingEntry = finalReferenceList.Concat(restOfReferenceList).ToList();
            allFinalRankingEntry = allFinalRankingEntry.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                allFinalRankingEntry[i].Position = i;
            }
            Result.FinalRanking = new FinalRanking(allFinalRankingEntry);
        }

        private (double[] deepCopyofMin, double[] deepCopyofMax) CalculateRangeOfValues(double[,] restrictionsMatrix, Alternative exampleAlternative, double[] arrayOfValues)
        {
            double[] min = new double[restrictionsMatrix.GetLength(1) - 1];
            double[] max = new double[restrictionsMatrix.GetLength(1) - 1];
            
            for (int i = 0; i < restrictionsMatrix.GetLength(1) - 1; i++)
            {
                min[i] = 0;
                max[i] = 1;
            }
            var arrayOfCriterion = exampleAlternative.CriteriaValues.Keys.ToArray();
            var criterionEndPoints = new List<int>();
            var count = 0;
            for (int i = 0; i < arrayOfCriterion.Length; i++)
            {
                count += arrayOfCriterion[i].LinearSegments;
                criterionEndPoints.Add(count);
            }

            double localMin,  localMax;
            
            
            for (var row = 0; row < restrictionsMatrix.GetLength(0); row++)
            {
                count = 0;
                for (var numOfCriterion = 0; numOfCriterion < arrayOfCriterion.Length; numOfCriterion++)
                {
                    for (int i = 0; i < arrayOfCriterion[numOfCriterion].LinearSegments; i++)
                    {
                        if (i == arrayOfCriterion[numOfCriterion].LinearSegments - 1)
                        {
                            ( localMin,  localMax) = CalculateValue(GetRow(restrictionsMatrix, row), count, arrayOfValues, false);

                        }
                        else
                        {
                            ( localMin,  localMax) = CalculateValue(GetRow(restrictionsMatrix, row), count, arrayOfValues, true);
                            
                        }
                        if (min[count] < localMin) min[count] = localMin;
                        if (max[count] > localMax) max[count] = localMax;
                        count++;
                    }
                }
            }
            double[] deepCopyofMin = new double[min.Length];
            double[] deepCopyofMax = new double[max.Length];
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
                {
                    if (element != innerElement)
                    {
                        if (endPointsMax > max[innerElement] - arrayOfValues[innerElement]) endPointsMax = max[innerElement] - arrayOfValues[innerElement];
                        if (endPointsMin < arrayOfValues[innerElement] - min[innerElement]) endPointsMin = arrayOfValues[innerElement] - min[innerElement];
                    }
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
            var min = (double)0;
            var max = (double)1;
            var sum = (double)0;

            
            if (row[index] == 0)
            {
                return (min, max);

            }
            else if(row[index] > 0)
            {
                for (var i = 0; i < row.Length - 1; i++)
                    if (i != index) sum -= arrayOfValues[i] * row[i];
                sum += arrayOfValues[row.Length - 1];
                sum /= row[index];
                if(hasNext)
                    if (max > row[index] + row[index + 1])
                    {
                        max = row[index] + row[index + 1];
                    }
                return (sum, max);
            }
            else
            {
                for (var i = 0; i < row.Length - 1; i++)
                    if (i != index) sum -= arrayOfValues[i] * row[i];
                sum += arrayOfValues[row.Length - 1];
                sum /= -row[index];
                if (hasNext)
                    if (sum > row[index] + row[index + 1])
                    {
                        sum = row[index] + row[index + 1];
                    }
                return (min, sum);
            }
        }


        private double[,] CalculateRestrictions(double[,] recalculatedMatrix, FinalRanking finalRanking)
        {
            double[,] matrix = new double[recalculatedMatrix.GetLength(0) - 1, recalculatedMatrix.GetLength(1) + 1];

            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < CriterionFieldsCount; c++) matrix[r, c] = recalculatedMatrix[r, c] - recalculatedMatrix[r + 1, c];
                if (finalRanking.FinalRankingList[r + 1].Utility - finalRanking.FinalRankingList[r].Utility > 0.05)
                    matrix[r, matrix.GetLength(1)] = 0;
                else
                    matrix[r, matrix.GetLength(1)] = 0.05;
            }

            return matrix;
        }

        public double[,] RecreateMatrix(FinalRanking finalRanking)
        {
            List<FinalRankingEntry> finalRankingEntryList = finalRanking.FinalRankingList;
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
            {
                for (var c = 0; c < matrix1.GetLength(1); c++)
                {
                    lengthBetweenMatrix += Math.Abs(matrix1[row, c] - matrix2[row, c]);
                }
            }

            lengthBetweenMatrix /= 2;
            tau = 1 - (4 * lengthBetweenMatrix) /
                  (finalReferenceRanking.FinalRankingList.Count * (finalReferenceRanking.FinalRankingList.Count - 1));
            return tau;
        }

        private float[,] CreateKendallMatrix()
        {
            float[,] rankingMatrix = new float[referenceRanking.ReferenceRankingList.Count, referenceRanking.ReferenceRankingList.Count];
            for (var row = 0; row < rankingMatrix.GetLength(0); row++)
            {
                for (var c = 0; c < rankingMatrix.GetLength(1); c++)
                {
                    if(row == c || referenceRanking.ReferenceRankingList[c].Rank - referenceRanking.ReferenceRankingList[row].Rank > 0) rankingMatrix[row, c] = 0;
                    if (referenceRanking.ReferenceRankingList[c].Rank - referenceRanking.ReferenceRankingList[row].Rank == 0) rankingMatrix[row, c] = 0.5F;
                    rankingMatrix[row, c] = 1;
                }
            }

            return rankingMatrix;
        }

        private float[,] CreateKendallMatrix(FinalRanking ranking)
        {
            float[,] rankingMatrix = new float[ranking.FinalRankingList.Count, ranking.FinalRankingList.Count];
            for (var row = 0; row < rankingMatrix.GetLength(0); row++)
            {
                for (var c = 0; c < rankingMatrix.GetLength(1); c++)
                {
                    if (row == c || ranking.FinalRankingList[c].Utility - ranking.FinalRankingList[row].Utility > 0.05) rankingMatrix[row, c] = 0;
                    if (ranking.FinalRankingList[c].Utility - ranking.FinalRankingList[row].Utility <= 0.05) rankingMatrix[row, c] = 0.5F;
                    rankingMatrix[row, c] = 1;
                }
            }

            return rankingMatrix;
        }

        private List<FinalRankingEntry> CreateRanking(double[] arrayOfValues, double[,] transientMatrix, List<Alternative> listOfAlternatives)
        {
            List<FinalRankingEntry> finalRankingList = new List<FinalRankingEntry>();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                var score = CreateFinalRankingEntryUtility(arrayOfValues, GetRow(transientMatrix, i));
                var finalRankingEntry = new FinalRankingEntry(-1, listOfAlternatives[i], (float) score);
                finalRankingList.Add(finalRankingEntry);
            }

            List<FinalRankingEntry> finalRankingSorted = finalRankingList.OrderBy(o => o.Utility).ToList();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                finalRankingSorted[i].Position = i;
            }

            return finalRankingSorted;
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
            double[] partialArray = new double[] { }, arrayOfValues = new double[] {};
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
            double[] newArray = new double[arrayOfValues.Length + partialArray.Length];
            for (var i = 0; i < arrayOfValues.Length; i++)
            {
                newArray[i] = arrayOfValues[i];
            }
            for(var i = arrayOfValues.Length; i < arrayOfValues.Length + partialArray.Length; i++)
            {
                newArray[i] = partialArray[i];
            }

            return newArray;
        }

        private (int, List<PartialUtilityValues>, double[]) NextPartialUtility(Dictionary<double, double> doubles, Criterion criterion, int count)
        {
            var linearSegments = criterion.LinearSegments;
            var arrayOfValues = new double[criterion.LinearSegments];
            List<PartialUtilityValues> list;
            double segmentValue = (criterion.MaxValue - criterion.MinValue) / linearSegments, currentPoint, currentValue = 0;
            if (criterion.CriterionDirection == "g")
            {
                currentPoint = criterion.MinValue;
                PartialUtilityValues partialUtilityValues = new PartialUtilityValues((float)currentPoint, (float)currentValue);
                list = new List<PartialUtilityValues> { partialUtilityValues };
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MinValue + s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
                    }
                    partialUtilityValues = new PartialUtilityValues((float)currentPoint, (float)currentValue);
                    list.Add(partialUtilityValues);
                    count++;
                }
            }
            else
            {
                currentPoint = criterion.MaxValue;
                PartialUtilityValues partialUtilityValues = new PartialUtilityValues((float)currentPoint, (float)currentValue);
                list = new List<PartialUtilityValues> { partialUtilityValues };
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MaxValue - s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
                    }
                    partialUtilityValues = new PartialUtilityValues((float)currentPoint, (float)currentValue);
                    list.Add(partialUtilityValues);
                    count++;
                }
            }
            return (count, list, arrayOfValues);
        }


        public double[,] CreateSimplexMatrix(int height, int width, double[,] matrix)
        {
            double[,] simplexMatrix = new double[height, width];
            var widthWithoutSlack = width - height - equals.Count;
            double[] cj = new double[width];
            int[] basic = new int[height];
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
                if(c < bound)
                    cj[c] = c < bound ? 0 : Double.PositiveInfinity;

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
            double[] row = new double[width];
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