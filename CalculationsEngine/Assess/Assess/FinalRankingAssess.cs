using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataModel.Results;
using DataModel.Structs;

namespace CalculationsEngine.Assess.Assess
{
    public class FinalRankingAssess : FinalRanking
    {
        public FinalRankingAssess()
        {
        }

        public FinalRankingAssess(List<AlternativeUtility> alternativesUtilitiesList)
        {
            alternativesUtilitiesList = alternativesUtilitiesList.OrderBy(o => o.Utility).ToList();

            for (var i = 0; i < alternativesUtilitiesList.Count; i++)
                finalRankingCollection.Add(new FinalRankingEntry(alternativesUtilitiesList.Count - i,
                    alternativesUtilitiesList[i].Alternative, alternativesUtilitiesList[i].Utility));
        }

        public FinalRankingAssess(ObservableCollection<FinalRankingEntry> finalRankingCollection) : base(finalRankingCollection)
        {
        }
    }
}