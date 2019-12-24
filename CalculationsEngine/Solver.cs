using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
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
        private Dictionary<double, double> solution;


        public Solver(ReferenceRanking referenceRanking, ReferenceRanking allVariantsRanking)
        {
            this.referenceRanking = referenceRanking;
            variantsList = new List<KeyValuePair<Alternative, int>>();
            foreach (var referenceRankingEntry in referenceRanking.ReferenceRankingList)
                variantsList.Add(new KeyValuePair<Alternative, int>(referenceRankingEntry.Alternative,
                    referenceRankingEntry.Rank));
            //variantsList = referenceRanking.AlternativeList;
            var cfc = 0;
            foreach (var criterion in variantsList[0].Key.CriteriaValues.Keys) cfc += criterion.LinearSegments;
            CriterionFieldsCount = cfc;
            equals = new List<int>();
            for (var i = 0; i < variantsList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);
        }

        private int CriterionFieldsCount { get; }
        private double[,] Matrix { get; set; }
        private Results Result { get; set; }
        private int[] BasicVariables { get; set; }

        private double[] Cj { get; set; }
        private int NumberOfIteration { get; set; }

        public void Calculate()
        {
            var heightOfPrimaryMatrix = variantsList[0].Key.CriteriaValues.Count;
            var height = HeightOfSimplexMatrix();
            var width = WidthOfSimplexMatrix(variantsList[0].Key);
            List<PartialUtility> partialUtilityList;
            double[] arrayOfValues;
            double[,] transientMatrix = CreateMatrix();
            Matrix = CreateSimplexMatrix(height, width, transientMatrix);
            float tau;

            var simplex =
                new Simplex(Matrix, BasicVariables, Cj); //widthOfPrimary = width - height, widthOfSlack = height
            simplex.Run(NumberOfIteration);
            solution = simplex.solution;
            (partialUtilityList, arrayOfValues) = MakePartialUtilityFunction(solution);
            List<FinalRankingEntry> finalReferenceList = CreateRanking(arrayOfValues, transientMatrix);
            FinalRanking finalReferenceRanking = new FinalRanking(finalReferenceList);
            tau = CalculateKendallCoeficient(finalReferenceRanking);
            Result.PartialUtilityFunctions = partialUtilityList;
            Result.FinalRanking = finalReferenceRanking;
            Result.KendallCoefficient = tau;
        }

        private float CalculateKendallCoeficient(FinalRanking finalReferenceRanking)
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

        private List<FinalRankingEntry> CreateRanking(double[] arrayOfValues, double[,] transientMatrix)
        {
            List<FinalRankingEntry> finalRankingList = new List<FinalRankingEntry>();
            for (var i = 0; i < transientMatrix.Length; i++)
            {
                var score = CreateFinalRankingEntryUtility(arrayOfValues, GetRow(transientMatrix, i));
                var finalRankingEntry = new FinalRankingEntry(-1, variantsList[i].Key, (float) score);
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
            Dictionary<float, float> dict;
            double[] partialArray = new double[] { }, arrayOfValues = new double[] {};
            var count = 0;
            foreach (var entry in variantsList[0].Key.CriteriaValues)
            {
                (count, dict, partialArray) = NextPartialUtility(doubles, entry.Key, count);
                arrayOfValues = AddTailToArray(arrayOfValues, partialArray);
                partialUtilityList.Add(new PartialUtility(entry.Key, dict));
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

        private (int, Dictionary<float, float>, double[]) NextPartialUtility(Dictionary<double, double> doubles, Criterion criterion, int count)
        {
            var linearSegments = criterion.LinearSegments;
            var arrayOfValues = new double[criterion.LinearSegments];
            Dictionary<float, float> dict;
            double segmentValue = (criterion.MaxValue - criterion.MinValue) / linearSegments, currentPoint, currentValue = 0;
            if (criterion.CriterionDirection == "g")
            {
                currentPoint = criterion.MinValue;
                dict = new Dictionary<float, float> { { (float)currentPoint, (float)currentValue } };
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MinValue + s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
                    }
                    dict.Add((float)currentPoint, (float)currentValue);
                    count++;
                }
            }
            else
            {
                currentPoint = criterion.MaxValue;
                dict = new Dictionary<float, float> { { (float)currentPoint, (float)currentValue } };
                for (var s = 1; s < linearSegments; s++)
                {
                    currentPoint = criterion.MaxValue - s * segmentValue;
                    arrayOfValues[s - 1] = 0;
                    if (doubles.Keys.Contains(count))
                    {
                        currentValue += doubles[count];
                        arrayOfValues[s - 1] = doubles[count];
                    }
                    arrayOfValues[s - 1] = currentValue;
                    dict.Add((float)currentPoint, (float)currentValue);
                    count++;
                }
            }
            return (count, dict, arrayOfValues);
        }


        public double[,] CreateSimplexMatrix(int height, int width, double[,] matrix)
        {
            double[,] simplexMatrix = new double[height, width];
            var widthWithoutSlack = width - height - equals.Count;
            for (var r = 0; r < variantsList.Count - 1; r++)
            {
                for (var c = 0; c < CriterionFieldsCount; c++) simplexMatrix[r, c] = matrix[r, c] - matrix[r + 1, c];
                simplexMatrix[r, r * 2 + CriterionFieldsCount] = -1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 1] = 1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 2] = 1;
                simplexMatrix[r, r * 2 + CriterionFieldsCount + 3] = -1;
            }

            //Create last ideal line = 1
            for (var c = 0; c < CriterionFieldsCount; c++)
                simplexMatrix[variantsList.Count - 1, c] = c < CriterionFieldsCount ? 1 : 0;
            //Add slack 1 and subtract surplus -1 variables.
            var i = 0;
            for (var r = 0; r < height; r++)
            {
                for (var c = widthWithoutSlack; c < widthWithoutSlack + height; c++)
                    if (equals.Contains(r) && c == widthWithoutSlack + i)
                    {
                        simplexMatrix[r, c] = equals.Contains(r) ? -1 : 0;
                        i++;
                    }
                for (var c = widthWithoutSlack + height; c < width; c++)
                    simplexMatrix[r, c] = c != r + widthWithoutSlack ? 0 : 1;
            }

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

        public double[,] CreateMatrix()
        {
            var matrix = new double[variantsList[0].Key.CriteriaValues.Count, CriterionFieldsCount];
            for (var i = 0; i < variantsList.Count; i++)
            {
                var row = GenerateRow(CriterionFieldsCount, variantsList[i].Key);
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