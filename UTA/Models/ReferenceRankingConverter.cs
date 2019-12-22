using DataModel.Input;
using DataModel.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReferenceRankingLists = UTA.Models.DataBase.ReferenceRanking;
using referenceRankingStructs = DataModel.Results.ReferenceRanking;

namespace UTA.Models
{
    public static class ReferenceRankingConverter
    {
        public static referenceRankingStructs ListToStruct(ReferenceRankingLists referenceRankingLists)
        {
            List<ReferenceRankingEntry> referenceRankingEntries = new List<ReferenceRankingEntry>();
            foreach (ObservableCollection<Alternative> rankingsCollection in referenceRankingLists.RankingsCollection)
            {
                foreach (Alternative alternative in rankingsCollection)
                {
                    if (alternative.ReferenceRank != null)
                    {
                        referenceRankingEntries.Add(new ReferenceRankingEntry(alternative.ReferenceRank.Value,
                            alternative));
                    }
                    else
                    {
                        throw new Exception(
                            "Found Alternative in reference ranking without initialized property ReferenceRank!");
                    }
                }
            }
            return new referenceRankingStructs(referenceRankingEntries);
        }

        public static ReferenceRankingLists StructToList(referenceRankingStructs referenceRankingStructs)
        {
            ReferenceRankingLists referenceRankingLists = new ReferenceRankingLists(0);
            foreach (ReferenceRankingEntry entry in referenceRankingStructs.ReferenceRankingList)
            {
                referenceRankingLists.AddAlternativeToRank(entry.Alternative, entry.Rank);
            }
            return referenceRankingLists;
        }
    }
}
