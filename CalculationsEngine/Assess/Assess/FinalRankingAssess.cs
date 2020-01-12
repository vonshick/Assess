using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;

namespace CalculationsEngine.Assess.Assess
{
    public class FinalRankingAssess : FinalRanking
    {
        public FinalRankingAssess() : base()
        {
        }

        public FinalRankingAssess(List<AlternativeUtility> alternativesUtilitiesList) : base()
        {
            alternativesUtilitiesList = alternativesUtilitiesList.OrderBy(o => o.Utility).ToList();

            for (int i = 0; i < alternativesUtilitiesList.Count; i++)
            {
                finalRankingCollection.Add(new FinalRankingEntry(alternativesUtilitiesList.Count - i, alternativesUtilitiesList[i].Alternative, (float)alternativesUtilitiesList[i].Utility));
            }
        }

        public FinalRankingAssess(ObservableCollection<FinalRankingEntry> finalRankingCollection) : base(finalRankingCollection)
        {
        }
    }
}
