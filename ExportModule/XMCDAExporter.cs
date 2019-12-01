using System;
using DataModel.Input;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ExportModule
{
    public class XMCDAExporter
    {
        public List<Criterion> CriterionList { get; set; }
        public List<Alternative> AlternativeList { get; set; }
        private string OutputDirectory { get; set; }
        private XmlTextWriter xmcdaWriter;

        public XMCDAExporter(string outputDirectory, List<Criterion> criterionList, List<Alternative> alternativeList) {
            OutputDirectory = outputDirectory;
            CriterionList = criterionList;
            AlternativeList = alternativeList;
        }

        // <xmcda:XMCDA 
        //         xmlns:xmcda="http://www.decision-deck.org/2016/XMCDA-3.0.2" 
        //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
        //         xsi:schemaLocation="http://www.decision-deck.org/2016/XMCDA-3.0.2 http://www.decision-deck.org/xmcda/_downloads/XMCDA-3.0.2.xsd"
        //     >
        // </xmcda:XMCDA>
        // 

        private void initializeWriter(string filePath)
        {
            xmcdaWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8);
            xmcdaWriter.Formatting = Formatting.Indented;
            xmcdaWriter.Indentation = 2;
            xmcdaWriter.WriteStartDocument(false);
            xmcdaWriter.WriteStartElement("xmcda:XMCDA");
            xmcdaWriter.WriteAttributeString("xmlns:xmcda", "http://www.decision-deck.org/2016/XMCDA-3.0.2");
            xmcdaWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xmcdaWriter.WriteAttributeString("xsi:schemaLocation", "http://www.decision-deck.org/2016/XMCDA-3.0.2 http://www.decision-deck.org/xmcda/_downloads/XMCDA-3.0.2.xsd");
        }

// TODO zmień na private!
        public void saveCriterions() {
            
            initializeWriter(Path.Combine(OutputDirectory, "criteria.xml"));
            xmcdaWriter.WriteStartElement("criteria");
            
            foreach(Criterion criterion in CriterionList) 
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteAttributeString("name", criterion.Name);
                xmcdaWriter.WriteAttributeString("id", criterion.ID);
                xmcdaWriter.WriteStartElement("active");
                xmcdaWriter.WriteString("true");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }
    }
}
