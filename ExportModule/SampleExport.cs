using System;
using System.Collections.Generic;
using System.IO;
using DataModel.Input;

namespace ExportModule
{
    public class SampleExport {
        public static void exportXMCDA(List<Criterion> criterionList, List<Alternative> alternativeList) 
        {
            string xmcdaOutputDirectory = Path.Combine(Environment.CurrentDirectory, "xmcda_output");
            XMCDAExporter xmcdaExporter = new XMCDAExporter(xmcdaOutputDirectory, criterionList, alternativeList);
            xmcdaExporter.saveSession();
        }
    }
}