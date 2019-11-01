using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class Ranking
    {
        public Ranking(List<Alternative> variantsList)
        {
            VariantsList = variantsList;
        }
        /// <summary> order of variants in list defines reference ranking order </summary>
        public List<Alternative> VariantsList { get; set; }
    }
}
