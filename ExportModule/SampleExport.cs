using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;
using System.Collections.Generic;
using System.IO;

namespace ExportModule
{
    public class SampleExport
    {
        private static List<PartialUtility> createSamplePartialUtilities(List<Criterion> criterionList)
        {
            var partialUtilities = new List<PartialUtility>();
            var pointsValuesList = new List<PartialUtilityValues>();
            var pointsValues1 = new PartialUtilityValues(0, 0, 0, 0);
            var pointsValues2 = new PartialUtilityValues(1, 0.5f, 0.55f, 0.6f);
            var pointsValues3 = new PartialUtilityValues(2, 0.8f, 0.55f, 1);
            pointsValuesList.Add(pointsValues1);
            pointsValuesList.Add(pointsValues2);
            pointsValuesList.Add(pointsValues3);

            foreach (var criterion in criterionList) partialUtilities.Add(new PartialUtility(criterion, pointsValuesList));

            return partialUtilities;
        }

        private static FinalRanking createSampleFinalRanking(List<Alternative> alternativeList)
        {
            var finalRakingList = new List<FinalRankingEntry>();
            
            for (int i = 0; i < alternativeList.Count; i++)
            {
                finalRakingList.Add(new FinalRankingEntry(i + 1, alternativeList[i], 1 / (i + 1)));
            }

            return new FinalRanking(finalRakingList);
        }

        public static void exportXMCDA(string dataDirectoryPath, List<Criterion> criterionList, List<Alternative> alternativeList)
        {
            var results = new Results();
            results.PartialUtilityFunctions = createSamplePartialUtilities(criterionList);
            results.FinalRanking = createSampleFinalRanking(alternativeList);

            string xmcdaOutputDirectory = Path.Combine(dataDirectoryPath, "xmcda_output");
            XMCDAExporter xmcdaExporter = new XMCDAExporter(xmcdaOutputDirectory,
                                                            criterionList,
                                                            alternativeList, 
                                                            results);
            xmcdaExporter.saveSession();
        }
    }
}