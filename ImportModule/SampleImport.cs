using DataModel.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ImportModule
{
    public class SampleImport
    {

        public static DataLoader ProcessSampleData(string dataDirectoryPath)
        {
            // XMLLoader dataLoader = new XMLLoader();
            // dataLoader.LoadData(Path.Combine(dataDirectoryPath, "sample.xml"));

            // UTXLoader dataLoader = new UTXLoader();
            // dataLoader.LoadData(Path.Combine(dataDirectoryPath, "utx_with_enum.utx"));

            CSVLoader dataLoader = new CSVLoader();
            dataLoader.LoadData(Path.Combine(dataDirectoryPath, "Zeszyt1.csv"));
//            dataLoader.LoadData(Path.Combine(dataDirectoryPath, "Lab7_bus.csv"));

            // XMCDALoader dataLoader = new XMCDALoader();
            // dataLoader.LoadData(Path.Combine(dataDirectoryPath, "xmcda"));

            Trace.WriteLine("### ### ### ### ### ### ### ### ### ###");
            Trace.WriteLine("Criteria:");
            for (int i = 0; i < dataLoader.CriterionList.Count; i++)
            {
                Trace.WriteLine("");
                Trace.WriteLine(dataLoader.CriterionList[i].Name);
                Trace.WriteLine(dataLoader.CriterionList[i].CriterionDirection);
                Trace.WriteLine("min:" + dataLoader.CriterionList[i].MinValue);
                Trace.WriteLine("max:" + dataLoader.CriterionList[i].MaxValue);
                Trace.WriteLine("");
            }

            Trace.WriteLine("Alternatives:");
            for (int i = 0; i < dataLoader.AlternativeList.Count; i++)
            {
                Trace.WriteLine(dataLoader.AlternativeList[i].Name);
                Dictionary<Criterion, float> dictionary = dataLoader.AlternativeList[i].CriteriaValues;
                foreach (KeyValuePair<Criterion, float> kvp in dictionary)
                {
                    Trace.WriteLine(kvp.Key.Name + " = " + kvp.Value);
                }
                Trace.WriteLine("");
            }

            Trace.WriteLine("### ### ### ### ### ### ### ### ### ###");
            Trace.WriteLine("");

            return dataLoader;
        }
    }
}
