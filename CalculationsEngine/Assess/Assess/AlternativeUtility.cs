using System;
using System.Collections.Generic;
using System.Text;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public struct AlternativeUtility
    {
        public Alternative Alternative;
        public double Utility;

        public AlternativeUtility(Alternative alternative, double utility)
        {
            Alternative = alternative;
            Utility = utility;
        }
    }
}
