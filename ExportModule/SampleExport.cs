using DataModel.Input;
using DataModel.Results;
using System.Collections.Generic;
using System.IO;


namespace ExportModule
{
    public class SampleExport
    {

        private static List<PartialUtility> createSamplePartialUtilities(List<Criterion> criterionList)
        {
            List<PartialUtility> partialUtilities = new List<PartialUtility>();
            List<PartialUtilityValues> pointsValuesList = new List<PartialUtilityValues>();
            PartialUtilityValues pointsValues1 = new PartialUtilityValues(0,0,0,0);
            PartialUtilityValues pointsValues2 = new PartialUtilityValues(1, 0.5f, 0.55f, 0.6f);
            PartialUtilityValues pointsValues3 = new PartialUtilityValues(2, 0.8f, 0.55f, 1);
            pointsValuesList.Add(pointsValues1);
            pointsValuesList.Add(pointsValues2);
            pointsValuesList.Add(pointsValues3);

            foreach (Criterion criterion in criterionList)
            {
                partialUtilities.Add(new PartialUtility(criterion, pointsValuesList));
            }

            return (partialUtilities);
        }

        private static List<KeyValuePair<Alternative, int>> createSampleResults(List<Alternative> alternativeList)
        {
            List<KeyValuePair<Alternative, int>> resultsList = new List<KeyValuePair<Alternative, int>>();
            for (int i = 0; i < alternativeList.Count; i++)
            {
                resultsList.Add(new KeyValuePair<Alternative, int>(alternativeList[i], i));
            }
            return (resultsList);
        }

        public static void exportXMCDA(string dataDirectoryPath, List<Criterion> criterionList, List<Alternative> alternativeList)
        {

            var partialUtilities = createSamplePartialUtilities(criterionList);
            var resultsList = createSampleResults(alternativeList);

            string xmcdaOutputDirectory = Path.Combine(dataDirectoryPath, "xmcda_output");
            XMCDAExporter xmcdaExporter = new XMCDAExporter(xmcdaOutputDirectory,
                                                            criterionList,
                                                            alternativeList,
                                                            resultsList,
                                                            partialUtilities);
            xmcdaExporter.saveSession();
        }
    }
}