using System;
using DataModel.Input;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ExportModule
{
    public class XMCDAExporter
    {
        private List<Criterion> criterionList;
        private List<Alternative> alternativeList;
        private string outputDirectory;
        private XmlTextWriter xmcdaWriter;

        public XMCDAExporter(string outputDirectory, List<Criterion> criterionList, List<Alternative> alternativeList) {
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
        }

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

        private void saveCriterions() {
            
            initializeWriter(Path.Combine(outputDirectory, "criteria.xml"));
            xmcdaWriter.WriteStartElement("criteria");
            
            foreach(Criterion criterion in criterionList) 
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteAttributeString("id", criterion.ID);
                xmcdaWriter.WriteAttributeString("name", criterion.Name);
                xmcdaWriter.WriteStartElement("active");
                xmcdaWriter.WriteString("true");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveCriterionScales() {
            
            initializeWriter(Path.Combine(outputDirectory, "criteria_scales.xml"));
            xmcdaWriter.WriteStartElement("criteriaScales");
            
            foreach(Criterion criterion in criterionList) 
            {
                xmcdaWriter.WriteStartElement("criterionScale");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(criterion.ID);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("scales");
                xmcdaWriter.WriteStartElement("scale");
                xmcdaWriter.WriteStartElement("quantitative");
                xmcdaWriter.WriteStartElement("preferenceDirection");
                xmcdaWriter.WriteString(criterion.CriterionDirection == "c" ? "min" : "max");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void savePerformanceTable() {
            
            initializeWriter(Path.Combine(outputDirectory, "performance_table.xml"));
            xmcdaWriter.WriteStartElement("performanceTable");
            xmcdaWriter.WriteAttributeString("mcdaConcept", "REAL");
            foreach(Alternative alternative in alternativeList) 
            {
                xmcdaWriter.WriteStartElement("alternativePerformances");
                xmcdaWriter.WriteStartElement("alternativeID");
                xmcdaWriter.WriteString(alternative.Name);
                xmcdaWriter.WriteEndElement();

                foreach (KeyValuePair<Criterion, float> criterionValuePair in alternative.CriteriaValues)
                {
                    xmcdaWriter.WriteStartElement("performance");
                    xmcdaWriter.WriteStartElement("criterionID");
                    xmcdaWriter.WriteString(criterionValuePair.Key.ID);
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteStartElement("values");
                    xmcdaWriter.WriteStartElement("value");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(criterionValuePair.Value.ToString());
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                }
                xmcdaWriter.WriteEndElement();
            }            

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();

        }

        public void saveSession()
        {
            saveCriterions();
            saveCriterionScales();
            savePerformanceTable();
        }
    }
}
