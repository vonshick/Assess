using System;
using System.Collections.Generic;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine
{
    internal class Solver
    {
        private readonly List<int> equals;
        private readonly ReferenceRanking referenceRanking;
        private readonly List<KeyValuePair<Alternative, int>> variantsList;
        private double[] solution;


        public Solver(ReferenceRanking referenceRanking)
        {
            variantsList = new List<KeyValuePair<Alternative, int>>();
            foreach (var r in referenceRanking.ReferenceRankingList)
                variantsList.Add(new KeyValuePair<Alternative, int>(r.Alternative, r.Rank));
            //variantsList = referenceRanking.AlternativeList;
            var cfc = CriterionFieldsCount;
            foreach (var c in variantsList[0].Key.CriteriaValues.Keys) cfc += c.LinearSegments;
            CriterionFieldsCount = cfc;
            equals = new List<int>();
            for (var i = 0; i < variantsList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);
        }

        private int CriterionFieldsCount { get; }
        private double[,] Matrix { get; set; }
        private int NumberOfIteration { get; set; }

        public void Calculate()
        {
            int height, width, heightOfPrimaryMatrix = variantsList[0].Key.CriteriaValues.Count;
            height = HeightOfSimplexMatrix();
            width = WidthOfSimplexMatrix(variantsList[0].Key);
            Matrix = CreateSimplexMatrix(height, width);

            var solutionOfSimplex =
                new Simplex(Matrix, variantsList.Count); //widthOfPrimary = width - height, widthOfSlack = height
            solution = solutionOfSimplex.Drive(NumberOfIteration);
            MakePartialUtilityFunction(solution);
        }

        private List<PartialUtility> MakePartialUtilityFunction(double[] doubles)
        {
            var floatArray = Array.ConvertAll(doubles, x => (float) x);
            var partialUtilityList = new List<PartialUtility>();
            int linearSegments, count = 0;
            Dictionary<float, float> dict;
            foreach (var entry in variantsList[0].Key.CriteriaValues)
            {
                dict = new Dictionary<float, float>();
                linearSegments = entry.Key.LinearSegments;
                dict.Add(0, 0);
                for (var i = 1; i < linearSegments + 1; i++) dict.Add(i, floatArray[count++]);
                partialUtilityList.Add(new PartialUtility(entry.Key, dict));
            }

            return partialUtilityList;
        }

        public double[,] CreateSimplexMatrix(int height, int width)
        {
            double[,] simplexMatrix = new double[height, width], matrix;
            matrix = CreateMatrix();
            var widthWithoutSlack = width - height;
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
            for (var r = 0; r < height; r++)
            for (var c = widthWithoutSlack; c < width; c++)
                simplexMatrix[r, c] = c != r + widthWithoutSlack ? 0 : r < variantsList.Count ? -1 : 1;
            simplexMatrix = AddPAndAnswerColumn(simplexMatrix, widthWithoutSlack);
            return simplexMatrix;
        }

        public double[,] AddPAndAnswerColumn(double[,] sM, int widthWithoutSlack)
        {
            int height = sM.GetLength(0), width = sM.GetLength(1) + 2;
            var simplexMatrix = new double[height, width];
            for (var r = 0; r < height; r++)
            {
                for (var c = 0; c < sM.GetLength(1); c++) simplexMatrix[r, c] = sM[r, c];
                simplexMatrix[r, sM.GetLength(1)] = r == height - 1 ? 1 : 0;
            }

            for (var r = 0; r < height; r++) simplexMatrix[r, sM.GetLength(1) + 1] = 0.05;
            //Copy equals
            int i = 0, col;
            for (var r = variantsList.Count; r < height; r++)
            {
                for (var c = 0; c < widthWithoutSlack; c++)
                {
                    simplexMatrix[r, c] = simplexMatrix[equals[i], c];
                    col = sM.GetLength(1) + 1;
                    if (r != height - 1)
                    {
                        simplexMatrix[r, col] = 0;
                        simplexMatrix[equals[i], col] = 0;
                    }
                    else
                    {
                        simplexMatrix[r, col] = 1;
                        simplexMatrix[equals[i], col] = 1;
                    }
                }

                i++;
            }

            return simplexMatrix;
        }

        public double[,] CreateMatrix()
        {
            var matrix = new double[variantsList[0].Key.CriteriaValues.Count, CriterionFieldsCount];
            double[] row;
            for (var i = 0; i < variantsList.Count; i++)
            {
                row = GenerateRow(CriterionFieldsCount, variantsList[i].Key);
                for (var j = 0; j < CriterionFieldsCount; j++) matrix[i, j] = row[j];
            }

            return matrix;
        }

        public double[] GenerateRow(int width, Alternative ae)
        {
            double[] row = new double[width], fields;
            var index = 0;
            foreach (var entry in ae.CriteriaValues)
            {
                fields = GenerateCriterionFields(entry);
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
            var cfc = CriterionFieldsCount;
            cfc += (alternative.CriteriaValues.Count - 1) * 2 + 4;
            cfc += HeightOfSimplexMatrix();
            return cfc;
        }

        public int HeightOfSimplexMatrix()
        {
            var hom = variantsList.Count;
            for (var i = 0; i < variantsList.Count - 1; i++)
                if (variantsList[i].Value == variantsList[i + 1].Value)
                    equals.Add(i);
            equals.Add(variantsList.Count - 1);
            hom += equals.Count;
            return hom;
        }
    }
}