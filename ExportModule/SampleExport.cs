using System;
using System.Collections.Generic;
using System.IO;
using DataModel.Input;
using DataModel.Results;


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

        private static List <KeyValuePair<Alternative, int>> createSampleResults(List<Alternative> alternativeList)
        {
            List < KeyValuePair<Alternative, int> > resultsList = new List<KeyValuePair<Alternative, int>>();
            for (int i = 0; i < alternativeList.Count; i++)
            {
                resultsList.Add(new KeyValuePair<Alternative, int> (alternativeList[i], i));
            }
            return(resultsList);
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