using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationsEngine
{
    class Simplex
    {
        private double[,] simplexMatrix;
        private int lowerBound;
        private int[] arrayOfStars, basicArray;
        private double[] solution;
        private List<PartialUtilityFunction> PartialUtilityList;

        public Simplex(double[,] simplexMatrix, int stars)
        {
            int height = simplexMatrix.GetLength(0), width = simplexMatrix.GetLength(1);
            this.simplexMatrix = new double[height + 1, width];
            this.basicArray = new int[height];
            this.lowerBound = width - height - 2;
            this.arrayOfStars = new int[stars];
            for (int i = 0; i < arrayOfStars.Length; i++)
            {
                arrayOfStars[i] = lowerBound + i;
            }
            for (var r = 0; r < height; r++)
            {
                for (var c = 0; c < width; c++)
                {
                    this.simplexMatrix[r, c] = simplexMatrix[r, c];
                }
            }

            //Extra row solve min problem => -1
            for (var c = 0; c < width; c++)
            {
                this.simplexMatrix[height, c] = (c < lowerBound) ? 0 : -1;
            }
            this.simplexMatrix[height, width - 1] = 1;
            this.simplexMatrix[height, width] = 0;
        }

        internal double[] Drive(int v)
        {
            var finish = CheckFinish();
            for (var i = 0; i < v; i++)
            {
                if (finish) break;
                var (row, col) = Pivot();
                if (row > -1)
                {
                    //calculateRow(row, col);
                    CalculateColAndRows(row, col);
                    UpdateBasicVar(row, col);
                }
                finish = CheckFinish();
            }
            return solution = (finish == true) ? null : ReadSolution();
        }

        private double[] ReadSolution()
        {
            var solution = new double[lowerBound - 1];
            for (var i = 0; i < basicArray.Length; i++)
            {
                solution[i] = simplexMatrix[i, simplexMatrix.GetLength(1) - 1] / simplexMatrix[i, basicArray[i]];
            }
            return solution;
        }

        private void UpdateBasicVar(int row, int col)
        {
            this.basicArray[row] = col;
        }

        private bool CheckFinish()
        {
            var finish = true;
            for (var c = 0; c < simplexMatrix.GetLength(1) - 1; c++)
            {
                if (this.simplexMatrix[simplexMatrix.GetLength(0) - 1, c] < 0) finish = false;
            }
            foreach (var t in basicArray)
            {
                if (arrayOfStars.Contains(t)) finish = false;
            }
            return finish;
        }

        private void CalculateColAndRows(int row, int col)
        {
            for (var r = 0; r < simplexMatrix.GetLength(0); r++)
            {
                if (r == row) continue;
                var num = this.simplexMatrix[row, col] / Math.Abs(this.simplexMatrix[r, col]);
                for (var c = 0; c < simplexMatrix.GetLength(1); c++)
                {
                    if (this.simplexMatrix[r, col] > 0)
                    {
                        this.simplexMatrix[r, c] = num * simplexMatrix[r, c] - this.simplexMatrix[row, c];
                    }
                    else
                    {
                        this.simplexMatrix[r, c] = num * simplexMatrix[r, c] + this.simplexMatrix[row, c];
                    }
                }
            }
        }

        private void CalculateRow(int row, int col)
        {
            var num = this.simplexMatrix[row, col];
            for (var c = 0; c < simplexMatrix.GetLength(1); c++)
            {
                this.simplexMatrix[row, c] = this.simplexMatrix[row, c] / num;
            }
            this.simplexMatrix[row, col] = 1;
        }

        private (int row, int col) Pivot()
        {
            int col = -1, row = -1;
            double min = 0, calc = 0;

            //col - largest pos val in row first star / bottom
            for (var i = 0; i < basicArray.GetLength(0); i++)
            {
                if (arrayOfStars.Contains(basicArray[i]))
                {
                    col = basicArray[i];
                    break;
                }
            }
            if (col == -1)
            {
                for (var c = 0; c < simplexMatrix.GetLength(1) - 1; c++)
                {
                    if (this.simplexMatrix[simplexMatrix.GetLength(0) - 1, c] < min)
                    {
                        min = this.simplexMatrix[simplexMatrix.GetLength(0) - 1, c];
                        col = c;
                    }
                }
            }

            if (col == -1) return (row, col);
            //row -  test ratio
            min = 0;
            for (var r = 0; r < simplexMatrix.GetLength(0) - 1; r++)
            {
                if (this.simplexMatrix[r, col] > 0)
                {
                    calc = this.simplexMatrix[r, simplexMatrix.GetLength(1) - 1] / this.simplexMatrix[r, col];
                    if (min == 0)
                    {
                        min = calc;
                        row = r;
                    }
                    else
                    {
                        if (calc < min)
                        {
                            min = calc;
                            row = r;
                        }
                    }
                }
            }
            return (row, col);
        }


    }
}
