using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalculationsEngine
{
    class Solver
    {
        private readonly List<KeyValuePair<Alternative, int>> VariantsList;
        private readonly Ranking ReferenceRanking;
        private List<int> Equals;
        private int CriterionFieldsCount { get; set; }
        private double[,] Matrix { get; set; }


        public Solver(Ranking referenceRanking)
        {
            VariantsList = referenceRanking.VariantsList;
            ReferenceRanking = referenceRanking;
            int cfc = CriterionFieldsCount;
            foreach (var c in referenceRanking.VariantsList[0].Key.CriteriaValues.Keys)
            {
                cfc += c.LinearSegments;
            }
            CriterionFieldsCount = cfc;
        }

        public void Calculate()
        {
            int height, width;
            height = HeightOfSimplexMatrix();
            width = WidthOfSimplexMatrix(VariantsList[0].Key);
            Matrix = CreateSimplexMatrix(height, width);

        }

        public double[,] CreateSimplexMatrix(int height, int width)
        {
            double[,] simplexMatrix = new double[height, width], matrix;
            matrix = CreateMatrix();
            int widthWithoutSlack = width - height;
            for (var r = 0; r < VariantsList.Count - 1; r++)
            {
                for (int c = 0; c < CriterionFieldsCount; c++)
                {
                    simplexMatrix[r, c] = matrix[r, c] - matrix[r + 1, c];
                }
                simplexMatrix[r, (r * 2 + CriterionFieldsCount)] = -1;
                simplexMatrix[r, (r * 2 + CriterionFieldsCount + 1)] = 1;
                simplexMatrix[r, (r * 2 + CriterionFieldsCount + 2)] = 1;
                simplexMatrix[r, (r * 2 + CriterionFieldsCount + 3)] = -1;
            }
            //Create last ideal line = 1
            for (int c = 0; c < CriterionFieldsCount; c++)
            {
                simplexMatrix[VariantsList.Count - 1, c] = ((c < CriterionFieldsCount) ? 1 : 0);
            }
            //Add slack 1 and subtract surplus -1 variables.
            for (int r = 0; r < height; r++)
            {
                for (int c = widthWithoutSlack; c < width; c++)
                {
                    simplexMatrix[r, c] = ((c != r + widthWithoutSlack) ? 0 : ((r < VariantsList.Count) ? -1 : 1));
                }
            }
            //solution col
            for (int r = 0; r < simplexMatrix.GetLength(0) - 1; r++)
            {
                simplexMatrix[r, simplexMatrix.GetLength(1) - 1] = 0.05;
            }
            //Copy equals
            int i = 0;
            for (int r = VariantsList.Count; r < height; r++)
            {
                for (int c = 0; c < widthWithoutSlack; c++)
                {
                    simplexMatrix[r, c] = simplexMatrix[Equals[i], c];
                    if (r != height - 1)
                    {
                        simplexMatrix[r, simplexMatrix.GetLength(1) - 1] = 0;
                        simplexMatrix[Equals[i], simplexMatrix.GetLength(1) - 1] = 0;
                    }
                    else
                    {
                        simplexMatrix[r, simplexMatrix.GetLength(1) - 1] = 1;
                        simplexMatrix[Equals[i], simplexMatrix.GetLength(1) - 1] = 1;
                    }
                }
                i++;
            }
            return simplexMatrix;
        }

        public double[,] CreateMatrix()
        {
            double[,] matrix = new double[VariantsList[0].Key.CriteriaValues.Count, CriterionFieldsCount];
            double[] row;
            for (int i = 0; i < VariantsList.Count; i++)
            {
                row = GenerateRow(CriterionFieldsCount, VariantsList[i].Key);
                for (int j = 0; j < CriterionFieldsCount; j++)
                {
                    matrix[i, j] = row[j];
                }
            }
            return matrix;
        }

        public double[] GenerateRow(int width, Alternative ae)
        {
            double[] row = new double[width], fields;
            int index = 0;
            foreach(KeyValuePair<Criterion, float> entry in ae.CriteriaValues)
            {
                fields = GenerateCriterionFields(entry);
                for (int j = 0; j < fields.Length; j++)
                {
                    row[index++] = fields[j];
                }
            }
            return row;
        }

        public double[] GenerateCriterionFields(KeyValuePair<Criterion, float> av)
        {
            int linearSegments = av.Key.LinearSegments;
            double segmentValue = (av.Key.MaxValue - av.Key.MinValue) / linearSegments, lowerBound, upperBound;
            double[] fields = new double[linearSegments];
            if (av.Key.CriterionDirection == "???asc") //TODO
            {
                lowerBound = av.Key.MinValue;
                for (int s = 0; s < linearSegments; s++)
                {
                    upperBound = av.Key.MinValue + (s + 1) * segmentValue;
                    if(av.Value < upperBound)
                    {
                        if(av.Value <= lowerBound)
                        {
                            fields[s] = 0;
                        }
                        else
                        {
                            fields[s] = (av.Value - lowerBound) / (upperBound - lowerBound);
                            if ( s > 0)
                            {
                                fields[s - 1] = 1 - fields[s];
                            }
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

            }
            return fields;
        }

        public int WidthOfSimplexMatrix(Alternative alternative)
        {
            int cfc = CriterionFieldsCount;
            cfc += (alternative.CriteriaValues.Count - 1) * 2 + 4;
            cfc += HeightOfSimplexMatrix();
            return cfc;
        }

        public int HeightOfSimplexMatrix()
        {
            int hom = VariantsList.Count;
            for(int i = 0; i < VariantsList.Count - 1; i++)
            {
                if (VariantsList[i].Value == VariantsList[i+1].Value) Equals.Add(i);
            }
            Equals.Add(VariantsList.Count - 1);
            hom += Equals.Count;
            return hom;
        }
    }
}
