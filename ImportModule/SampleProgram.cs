using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using DataModel.Input;

namespace ImportModule
{
    public class SampleProgram
    {
        private static void ProcessXMCDA()
        {
            XMCDALoader xmcdaLoader = new XMCDALoader(Path.Combine(Environment.CurrentDirectory, "xmcda"));
            xmcdaLoader.LoadXMCDA();

            xmcdaLoader.setMinAndMaxCriterionValues();

            Trace.WriteLine("Criteria:");
            for (int i = 0; i < xmcdaLoader.CriterionList.Count; i++)
            {
                Trace.WriteLine("");
                Trace.WriteLine(xmcdaLoader.CriterionList[i].Name);
                Trace.WriteLine(xmcdaLoader.CriterionList[i].CriterionDirection);
                Trace.WriteLine("min:" + xmcdaLoader.CriterionList[i].MinValue);
                Trace.WriteLine("max:" + xmcdaLoader.CriterionList[i].MaxValue);
                Trace.WriteLine("");
            }

            Trace.WriteLine("Alternatives:");
            for (int i = 0; i < xmcdaLoader.AlternativeList.Count; i++)
            {
                Dictionary<Criterion, float> dictionary = xmcdaLoader.AlternativeList[i].CriteriaValues;
                foreach (KeyValuePair<Criterion, float> kvp in dictionary)
                {
                    Trace.WriteLine(kvp.Key.Name + " = " + kvp.Value);
                }
                Trace.WriteLine("");
            }

            Trace.WriteLine("### ### ### ### ### ### ### ### ### ###");
            Trace.WriteLine("");
        }

        // Set one of boolean variables to 'true' to process file with specific extension
        public static void ProcessSampleData(bool csv, bool utx, bool xml, bool xmcda) {
            if (xmcda)
            {
                Trace.WriteLine("#### XMCDA #####");
                ProcessXMCDA();
            }
            else
            {
                DataLoader dataLoader = new DataLoader();
                if (csv)
                {
                    Trace.WriteLine("#### CSV #####");
                    // dataLoader.LoadCSV("Zeszyt1.csv");
                    dataLoader.LoadCSV("Lab7_bus.csv");
                }
                else if (utx)
                {
                    Trace.WriteLine("#### UTX #####");
                    dataLoader.LoadUTX("utx_with_enum.utx");
                }
                else if (xml)
                {
                    Trace.WriteLine("#### XML #####");
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
            }

            Trace.WriteLine("### ### ### ### ### ### ### ### ### ###");
            Trace.WriteLine("");
        }
    }
}
