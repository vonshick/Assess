using System;
using System.Collections.Generic;
using System.Text;

namespace CalculationsEngine.Assess.Assess
{
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
}
