using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace ImportModule
{
    public class SampleProgram
    {
        private static void ProcessXMCDA()
        {
            XMCDALoader xmcdaLoader = new XMCDALoader();
            xmcdaLoader.LoadXMCDA(Path.Combine(Environment.CurrentDirectory, "xmcda"));

            Trace.WriteLine("Criteria:");
            for (int i = 0; i < xmcdaLoader.CriterionList.Count; i++)
            {
                Trace.WriteLine(xmcdaLoader.CriterionList[i].Name);
                Trace.WriteLine(xmcdaLoader.CriterionList[i].CriterionDirection);
                Trace.WriteLine("");
            }

            Trace.WriteLine("Alternatives:");
            for (int i = 0; i < xmcdaLoader.AlternativeList.Count; i++)
            {
                Dictionary<string, float> dictionary = xmcdaLoader.AlternativeList[i].CriteriaValues;
                foreach (KeyValuePair<string, float> kvp in dictionary)
                {
                    Trace.WriteLine(kvp.Key + " = " + kvp.Value);
                }
                Trace.WriteLine("");
            }
        }

        // Set one of boolean variables to 'true' to process file with specific extension
        public static void ProcessSampleData(bool csv, bool utx, bool xml, bool xmcda) {
            if (xmcda)
            {
                ProcessXMCDA();
            }
            else
            {
                DataLoader dataLoader = new DataLoader();
                if (csv)
                {
                    // dataLoader.LoadCSV("Zeszyt1.csv");
                    dataLoader.LoadCSV("Lab7_bus.csv");
                }
                else if (utx)
                {
                    dataLoader.LoadUTX("utx_with_enum.utx");
                }
                else if (xml)
                {
                    dataLoader.LoadXML("sample.xml");
                }
                else
                {
                    Trace.WriteLine("No file extension was chosen!");
                }


                dataLoader.setMinAndMaxCriterionValues();

                Trace.WriteLine("Criteria:");
                for (int i = 0; i < dataLoader.CriterionList.Count; i++)
                {
                    Trace.WriteLine(dataLoader.CriterionList[i].Name);
                }
                Trace.WriteLine("");

                Trace.WriteLine("Alternatives:");
                for (int i = 0; i < dataLoader.AlternativeList.Count; i++)
                {
                    Dictionary<string, float> dictionary = dataLoader.AlternativeList[i].CriteriaValues;
                    foreach (KeyValuePair<string, float> kvp in dictionary)
                    {
                        Trace.WriteLine(kvp.Key + " = " + kvp.Value);
                    }
                    Trace.WriteLine("");
                }
            }
        }
    }
}
