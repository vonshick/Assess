using System.Collections.Generic;
using DataModel.Input;

namespace DataModel.Results
{
    public class Ranking
    {
        public Ranking()
        {
            VariantsList = new List<KeyValuePair<AlternativeEntry, int>>();
        }
        public Ranking(List<KeyValuePair<AlternativeEntry, int>> variantsList)
        {
            VariantsList = variantsList;
        }

        public void addVariant(AlternativeEntry alternative, int rank)
        {
            VariantsList.Add(new KeyValuePair<AlternativeEntry, int>(alternative, rank));
        }
        /// <summary> order of variants in list defines reference ranking order </summary>
        public List<KeyValuePair<AlternativeEntry, int>> VariantsList { get; set; }
    }
}
