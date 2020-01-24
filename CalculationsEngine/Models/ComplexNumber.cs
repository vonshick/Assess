namespace CalculationsEngine.Models
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
