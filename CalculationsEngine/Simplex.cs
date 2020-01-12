using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculationsEngine
{
    internal class Simplex
    {
        private readonly int[] basicVariables;
        private readonly double[] Cj;
        private readonly double[] CjZj;
        private readonly double[] CjZjInfinity;
        private double[,] simplexMatrix;


        public Simplex(double[,] matrix, int[] basic, double[] cj)
        {
            simplexMatrix = matrix;
            basicVariables = basic;
            Cj = cj;
            CjZj = new double[Cj.Length];
            CjZjInfinity = new double[Cj.Length];
            solution = new Dictionary<double, double>();
        }

        public Dictionary<double, double> solution { get; set; }

        internal void Run(int v)
        {
            for (var iteration = 0; iteration < v; iteration++)
            {
                CalculateCjZjAndInfinity();
                if (CheckFinish())
                {
                    SaveSolution();
                    break;
                }

                var pivotCol = GetIndexOfMostNegativeCjZj();
                var testRatio = TestRatio(pivotCol);
                var pivotRow = Array.IndexOf(testRatio, testRatio.Min());
                CalculateMatrix(pivotRow, pivotCol);
                UpdateBasicVariables(pivotRow, pivotCol);
            }
        }

        private void SaveSolution()
        {
            for (var row = 0; row < simplexMatrix.GetLength(0); row++)
                solution.Add(basicVariables[row], simplexMatrix[row, simplexMatrix.GetLength(1) - 1]);
        }

        private void UpdateBasicVariables(int pivotRow, int pivotCol)
        {
            basicVariables[pivotRow] = pivotCol;
        }

        private void CalculateMatrix(int pivotRow, int pivotCol)
        {
            var temporaryMatrix = new double[simplexMatrix.GetLength(0), simplexMatrix.GetLength(1)];
            for (var row = 0; row < simplexMatrix.GetLength(0); row++)
            for (var column = 0; column < simplexMatrix.GetLength(1); column++)
                if (pivotRow == row)
                    temporaryMatrix[pivotRow, column] = Math.Round(simplexMatrix[pivotRow, column] / simplexMatrix[pivotRow, pivotCol], 3);
                else if (pivotCol == column) temporaryMatrix[row, pivotCol] = 0;
                else
                    temporaryMatrix[row, column] =
                        Math.Round(
                            simplexMatrix[row, column] - simplexMatrix[pivotRow, column] * simplexMatrix[row, pivotCol] /
                            simplexMatrix[pivotRow, pivotCol], 3);

            simplexMatrix = temporaryMatrix;
        }

        private bool CheckFinish()
        {
            var finish = true;
            for (var column = 0; column < CjZj.Length; column++)
                if (CjZjInfinity[column] < 0)
                {
                    finish = false;
                }
                else
                {
                    if (CjZj[column] < 0 && CjZjInfinity[column] == 0) finish = false;
                }

            return finish;
        }

        private void CalculateCjZjAndInfinity()
        {
            for (var column = 0; column < CjZj.Length; column++)
            {
                if (double.IsInfinity(Cj[column]))
                {
                    CjZjInfinity[column] = 1;
                    CjZj[column] = 0;
                }
                else
                {
                    CjZjInfinity[column] = 0;
                    CjZj[column] = Cj[column];
                }

                for (var columnElement = 0; columnElement < simplexMatrix.GetLength(0); columnElement++)
                {
                    var CiElement = Cj[basicVariables[columnElement]];
                    if (double.IsInfinity(CiElement))
                        CjZjInfinity[column] -= simplexMatrix[columnElement, column];
                    else
                        CjZj[column] -= Math.Round(simplexMatrix[columnElement, column] * CiElement, 3);
                }
            }
        }

        private int GetIndexOfMostNegativeCjZj()
        {
            var min = double.PositiveInfinity;
            var index = -1;
            var list = new List<int>();
            for (var i = 0; i < CjZjInfinity.Length; i++)
            {
                if (min > CjZjInfinity[i])
                {
                    min = CjZjInfinity[i];
                    index = i;
                    list.Clear();
                }

                if (min == CjZjInfinity[i]) list.Add(i);
            }

            if (list.Count == 1) return index;
            min = double.PositiveInfinity;
            if (list.Count > 1)
            {
                for (var i = 0; i < list.Count; i++)
                    if (min > CjZj[list[i]])
                    {
                        min = CjZj[list[i]];
                        index = list[i];
                    }

                return index;
            }

            min = double.PositiveInfinity;
            for (var i = 0; i < CjZj.Length; i++)
                if (min > CjZj[i])
                {
                    min = CjZj[i];
                    index = i;
                }

            return index;
        }

        private double[] TestRatio(int col)
        {
            var ratio = new double[simplexMatrix.GetLength(0)];
            for (var i = 0; i < simplexMatrix.GetLength(0); i++)
                if (simplexMatrix[i, col] > 0)
                    ratio[i] = simplexMatrix[i, simplexMatrix.GetLength(1) - 1] / simplexMatrix[i, col];
                else ratio[i] = double.PositiveInfinity;

            return ratio;
        }
    }
}