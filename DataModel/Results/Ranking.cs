using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class Ranking
    {
        public Ranking()
        {
        }
        public Ranking(List<KeyValuePair<Alternative, int>> variantsList)
        {
            VariantsList = variantsList;
        }

        public void AddVariant(Alternative alternative, int rank)
        {
            VariantsList.Add(new KeyValuePair<Alternative, int>(alternative, rank));
        }
        /// <summary> order of variants in list defines reference ranking order </summary>
        public List<KeyValuePair<Alternative, int>> VariantsList { get; set; }
    }
}
