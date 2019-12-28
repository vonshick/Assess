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
            List<PartialUtility> partialUtilities = new List<PartialUtility>();
            Dictionary<float, float> pointsValues = new Dictionary<float, float>();
            pointsValues.Add(1, 1);
            pointsValues.Add(2, 2);
            pointsValues.Add(3, 4);

            foreach (Criterion criterion in criterionList)
            {
                partialUtilities.Add(new PartialUtility(criterion, pointsValues));
            }

            return (partialUtilities);
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