using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationsEngine.Assess.Assess
{
    public class BaristowSolver
    {
        private int n;
        private double[] a;
        private int mit;
        private double mincorr;
        private double zerodet;
        public ComplexNumber[] z;
        public ComplexNumber[] w;
        public int it;
        public int st;

        public BaristowSolver(double[] coefficents)
        {
            a = coefficents;
            n = a.Length - 1;
            mit = (int)1E6;
            mincorr = 1E-63;
            zerodet = 1E-63;
            z = createComplexNumberArray(n);
            w = createComplexNumberArray(n);
        }

        public struct ComplexNumber
        {
            public double im;
            public double re;

            public ComplexNumber(double im, double re)
            {
                this.im = im;
                this.re = re;
            }
        }

        private ComplexNumber[] createComplexNumberArray(int n)
        {
            List<ComplexNumber> ComplexNumberList = new List<ComplexNumber>();

            for (int i = 0; i <= n; i++)
            {
                ComplexNumberList.Add(new ComplexNumber(0, 0));
            }

            return ComplexNumberList.ToArray();
        }

        private void solve()
        {
            int i, k, n1;

            double m, p, pq0, pq1, q, q0, q1, q2, q3, q4; //git

            bool cond, endpq; //git

            double[] b = new double[n + 1];

            if (n < 1 || mit < 1 || mincorr <= 0 || zerodet <= 0)
            {
                st = 1;
            }
            else
            {
                for (i = 0; i <= n; i++)
                {
                    b[n - i] = a[i];
                }

                st = 0;
                i = 1;
                p = -1;
                q = -1;
                n1 = n - 1;
                cond = true;
                it = 0;

                do
                {
                    if (n == 1)
                    {
                        z[i].re = - b[1] / b[0];
                        z[i].im = 0;
                        cond = false;
                    }
                    else
                    {
                        if (n == 2)
                        {
                            q = b[0];
                            p = b[1] / q;
                            q = b[2] / q;
                            cond = false;
                        }
                        else
                        {
                            pq0 = 1E63;
                            pq1 = pq0;
                            endpq = true;
                            do
                            {
                                it = it + 1;
                                q0 = 0;
                                q1 = 0;
                                q2 = b[0];
                                q3 = b[1] - p * q2;
                                for (k = 2; k <= n; k++)
                                {
                                    q4 = b[k] - p * q3 - q * q2;
                                    q2 = q2 - p * q1 - q * q0;
                                    q0 = q1;
                                    q1 = q2;
                                    q2 = q3;
                                    q3 = q4;
                                }

                                if ((Math.Abs(q2) + Math.Abs(q3)) < zerodet)
                                {
                                    endpq = false;
                                }
                                else
                                {
                                    m = q * q0 + p * q1;
                                    q4 = q1 * q1 + m * q0;
                                    if (Math.Abs(q4) < zerodet)
                                    {
                                        st = 2;
                                    }
                                    else
                                    {
                                        q0 = (q1 * q2 - q0 * q3) / q4;
                                        q1 = (m * q2 + q1 * q3) / q4;
                                        q2 = Math.Abs(q0);
                                        q3 = Math.Abs(q1);
                                        if (q2 > mincorr || q3 > mincorr || q2 < pq0 && q3 < pq1)
                                        {
                                            p = p + q0;
                                            pq0 = q2;
                                            q = q + q1;
                                            pq1 = q3;
                                        }
                                        else
                                        {
                                            endpq = false;
                                        }

                                        if (it == mit && endpq)
                                        {
                                            st = 3;
                                        }
                                    }
                                }
                            } while (!(st != 0 || !endpq));

                            if (st == 2 || st == 3)
                            {
                                for (i = 1; i <= n; i++)
                                {
                                    z[i].re = 0;
                                    z[i].im = 0;
                                }

                                w = z;
                            }
                        }

                        // nie jestem pewny, czy poniższy if powinien zacząć się
                        // w tym bloku czy w następnym
                        if (st == 0)
                        {
                            m = -p / 2;
                            q0 = m * m - q;
                            q1 = Math.Sqrt(Math.Abs(q0));
                            if (q0 < 0)
                            {
                                z[i].re = m;
                                z[i + 1].re = m;
                                z[i].im = q1;
                                z[i + 1].im = -q1;
                            }
                            else
                            {
                                if (m > 0)
                                {
                                    m = m + q1;
                                }
                                else
                                {
                                    m = m - q1;
                                }

                                z[i + 1].re = m;

                                if (Math.Abs(m) == 0)
                                {
                                    z[i].re = 0;
                                }
                                else
                                {
                                    z[i].re = q / m;
                                }

                                z[i].im = 0;
                                z[i + 1].im = 0;
                            }

                            if (n > 2)
                            {
                                i = i + 2;
                                n = n - 2;
                                q0 = 0;
                                q1 = b[0];
                                for (k = 1; k <= n; k++)
                                {
                                    q2 = b[k] - p * q1 - q * q0;
                                    b[k] = q2;
                                    q0 = q1;
                                    q1 = q2;
                                }
                            }
                        }
                    }
                } while (!(st != 0 || !cond));

                if (st == 0)
                {
                    n = n1 + 1;
                    for (i = 1; i <= n; i++)
                    {
                        p = z[i].re;
                        q = z[i].im;
                        q1 = a[n];
                        if (q == 0)
                        {
                            for (k = n1; k >= 0; k--)
                            {
                                q1 = q1 * p + a[k];
                            }

                            q2 = 0;
                        }
                        else
                        {
                            q0 = Math.Sqrt(Math.Sqrt(p) + Math.Sqrt(q));
                            q3 = 2 * Math.Atan(q / (p + q0));
                            q4 = n * q3;
                            q2 = q1 * Math.Sin(q4);
                            q1 = q1 * Math.Cos(q4);

                            for (k = n1; k <= 0; k--)
                            {
                                p = a[k];
                                q4 = k * q3;
                                q1 = q1 * q0 + p * Math.Cos(q4);
                                q2 = q2 * q0 + p * Math.Sin(q4);
                            }
                        }

                        w[i].re = q1;
                        w[i].im = q2;
                    }
                }
            }
        }

        private double getSuitableRoot()
        {
            ComplexNumber minImComplexNumber = new ComplexNumber(double.PositiveInfinity, double.PositiveInfinity);

            // choose the complex number with min imaginary part and min index
            for (int i = z.Length - 1; i >= 1; i--)
                if (Math.Abs(z[i].im) <= Math.Abs(minImComplexNumber.im))
                    if(z[i].re != 0)
                        minImComplexNumber = z[i];
            
            return minImComplexNumber.re;
        }

        public double GetScallingCoefficient()
        {
            solve();
            return getSuitableRoot();
        }
    }
}
