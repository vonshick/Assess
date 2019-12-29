using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
//using DataModel.Results;

namespace CalculationsEngine
{
    internal class Simplex
    {
        private readonly double[] CjZj;
        private readonly double[] CjZjInfinity;
        private readonly int[] basicVariables;
        private readonly double[] Cj;
        private double[,] simplexMatrix;
        public Dictionary<double, double> solution { get; set; }


        public Simplex(double[,] matrix, int[] basic, double[] cj)
        {

            simplexMatrix = matrix;
            basicVariables = basic;
            Cj = cj;
            CjZj = new double[Cj.Length];
            CjZjInfinity = new double[Cj.Length];
            solution = new Dictionary<double, double>();
        }

        internal void Run(int v)
        {
            solution = null;
            for (var iteration = 0; iteration < v; iteration++)
            {

                CalculateCjZjAndInfinity();
                if (checkFinish() == true)
                {
                    saveSolution();
                    break;
                } 
                var pivotCol = GetIndexOfMostNegativeCjZj();
                var testRatio = TestRatio(pivotCol);
                var pivotRow = Array.IndexOf(testRatio, testRatio.Min());
                calculateMatrix(pivotRow, pivotCol);
                updateBasicVariables(pivotRow, pivotCol);

            }

        }

        private void saveSolution()
        {
            for (var row = 0; row < simplexMatrix.GetLength(0); row++)
            {
                solution.Add(basicVariables[row], simplexMatrix[basicVariables[row], simplexMatrix.GetLength(1)]);
            }
        }

        private void updateBasicVariables(int pivotRow, int pivotCol)
        {
            basicVariables[pivotRow] = pivotCol;
        }

        void calculateMatrix(int pivotRow, int pivotCol)
        {
            var temporaryMatrix = new double[simplexMatrix.GetLength(0), simplexMatrix.GetLength(1)];
            for (var row = 0; row < simplexMatrix.GetLength(0); row++)
            {
                for (var column = 0; column < simplexMatrix.GetLength(1); column++)
                {
                    if (pivotRow == row) temporaryMatrix[pivotRow, column] = Math.Round((simplexMatrix[pivotRow, column] / simplexMatrix[pivotRow, pivotCol]), 3);
                    else if (pivotCol == column) temporaryMatrix[row, pivotCol] = 0;
                    else temporaryMatrix[row, column] = Math.Round(simplexMatrix[row, column] - (simplexMatrix[pivotRow, column] * simplexMatrix[row, pivotCol] / simplexMatrix[pivotRow, pivotCol]), 3);
                }
            }

            simplexMatrix = temporaryMatrix;
        }

        void calculateMatrix2(int pivotRow, int pivotCol)
        {
            var temporaryMatrix = new double[simplexMatrix.GetLength(0), simplexMatrix.GetLength(1)];
            for (var row = 0; row < simplexMatrix.GetLength(0); row++)
            {
                for (var column = 0; column < simplexMatrix.GetLength(1); column++)
                {
                    if (pivotRow == row) temporaryMatrix[pivotRow, column] = Math.Round((simplexMatrix[pivotRow, column] / simplexMatrix[pivotRow, pivotCol]), 3);
                    else if (pivotCol == column) temporaryMatrix[row, pivotCol] = 0;
                    else
                    {
                        var value = simplexMatrix[row, pivotCol] / simplexMatrix[pivotRow, pivotCol];
                        temporaryMatrix[row, column] = Math.Round(simplexMatrix[row, column] - (simplexMatrix[pivotRow, column] * simplexMatrix[row, pivotCol] / simplexMatrix[pivotRow, pivotCol]), 3);
                    }
                }
            }

            simplexMatrix = temporaryMatrix;
        }

        bool checkFinish()
        {
            bool finish = true;
            for (var column = 0; column < CjZj.Length; column++)
            {
                if (CjZjInfinity[column] < 0)
                {
                    finish = false;
                }
                else
                {
                    if (CjZj[column] < 0 && CjZjInfinity[column] == 0) finish = false;
                }
            }

            return finish;
        }

        void CalculateCjZjAndInfinity()
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
                    {
                        CjZjInfinity[column] -= simplexMatrix[columnElement, column];
                    }
                    else
                    {
                        CjZj[column] -= Math.Round(simplexMatrix[columnElement, column] * CiElement, 3);
                    }
                }
            }
        }

        int GetIndexOfMostNegativeCjZj()
        {
            var min = Double.PositiveInfinity;
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
            min = Double.PositiveInfinity;
            if (list.Count > 1)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (min > CjZj[list[i]])
                    {
                        min = CjZj[list[i]];
                        index = list[i];
                    }
                }

                return index;
            }
            min = Double.PositiveInfinity;
            for (var i = 0; i < CjZj.Length; i++)
            {
                if (min > CjZj[i])
                {
                    min = CjZj[i];
                    index = i;
                }
            }
            return index;
        }

        double[] TestRatio(int col)
        {
            var ratio = new double[simplexMatrix.GetLength(0)];
            for (var i = 0; i < simplexMatrix.GetLength(0); i++)
            {
                if (simplexMatrix[i, col] > 0)
                    ratio[i] = simplexMatrix[i, simplexMatrix.GetLength(1) - 1] / simplexMatrix[i, col];
                else ratio[i] = Double.PositiveInfinity;
            }

            return ratio;
        }


        private void print(double[,] simplexMatrix)
        {
            int rowLength = simplexMatrix.GetLength(0);
            int colLength = simplexMatrix.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                Console.Write("basic: " + basicVariables[i] + " ");
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write(string.Format("{0} ", simplexMatrix[i, j]));
                }
                Console.Write(Environment.NewLine);
            }
            Console.ReadLine();
        }
        private void print(double[] array)
        {
            int rowLength = array.GetLength(0);

            for (int i = 0; i < rowLength; i++)
            {
                Console.Write(string.Format("{0} ", array[i]));

            }
            Console.Write(Environment.NewLine);
            Console.ReadLine();
        }
    }

}

