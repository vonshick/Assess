using DataModel.Input;
using DataModel.Results;
using DataModel.Structs;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ExportModule
{
    public class XMCDAExporter
    {
        private List<Criterion> criterionList;
        private List<Alternative> alternativeList;
        private Results results;
        private string outputDirectory;
        private XmlTextWriter xmcdaWriter;
        //TODO vonshick change two resultsList and partialUtilityList into Results object
        public XMCDAExporter(string outputDirectory,
                             List<Criterion> criterionList,
                             List<Alternative> alternativeList,
                             Results results)
        {
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
            this.results = results;
        }

        public XMCDAExporter(string outputDirectory,
                             List<Criterion> criterionList,
                             List<Alternative> alternativeList)
        {
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

        private void saveCriterions()
        {

            initializeWriter(Path.Combine(outputDirectory, "criteria.xml"));
            xmcdaWriter.WriteStartElement("criteria");

            foreach (Criterion criterion in criterionList)
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

        private void saveCriterionScales()
        {

            initializeWriter(Path.Combine(outputDirectory, "criteria_scales.xml"));
            xmcdaWriter.WriteStartElement("criteriaScales");

            foreach (Criterion criterion in criterionList)
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

        private void savePerformanceTable()
        {

            initializeWriter(Path.Combine(outputDirectory, "performance_table.xml"));
            xmcdaWriter.WriteStartElement("performanceTable");
            xmcdaWriter.WriteAttributeString("mcdaConcept", "REAL");
            foreach (Alternative alternative in alternativeList)
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

        private void saveRanking()
        {
            initializeWriter(Path.Combine(outputDirectory, "alternatives_ranks.xml"));
            xmcdaWriter.WriteStartElement("alternativesValues");

            foreach (FinalRankingEntry finalRankingEntry in results.FinalRanking.FinalRankingList)
            {
                xmcdaWriter.WriteStartElement("alternativeValue");
                xmcdaWriter.WriteStartElement("alternativeID");
                xmcdaWriter.WriteString(finalRankingEntry.Alternative.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("value");
                xmcdaWriter.WriteStartElement("integer");
                xmcdaWriter.WriteString(finalRankingEntry.Position.ToString());
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        public void saveCriteriaSegments()
        {
            initializeWriter(Path.Combine(outputDirectory, "criteria_segments.xml"));
            xmcdaWriter.WriteStartElement("criteriaValues");

            foreach (PartialUtility partialUtility in results.PartialUtilityFunctions)
            {
                xmcdaWriter.WriteStartElement("criterionValue");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(partialUtility.Criterion.ID);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("value");
                xmcdaWriter.WriteStartElement("integer");
                xmcdaWriter.WriteString(partialUtility.PointsValues.Count.ToString());
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveValueFunctions()
        {
            initializeWriter(Path.Combine(outputDirectory, "value_functions.xml"));
            xmcdaWriter.WriteStartElement("criteria");
            xmcdaWriter.WriteAttributeString("mcdaConcept", "criteria");

            foreach (PartialUtility partialUtility in results.PartialUtilityFunctions)
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(partialUtility.Criterion.ID);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("criterionFunction");
                xmcdaWriter.WriteStartElement("points");
                foreach (KeyValuePair<float, float> pointValue in partialUtility.PointsValues)
                {
                    xmcdaWriter.WriteStartElement("point");

                    xmcdaWriter.WriteStartElement("abscissa");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.Key.ToString());
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();

                    xmcdaWriter.WriteStartElement("ordinate");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.Value.ToString());
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();

                    xmcdaWriter.WriteEndElement();
                }

                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        public void saveInput()
        {
            saveCriterions();
            saveCriterionScales();
            savePerformanceTable();
        }

        public void saveResults()
        {
            if(results != null) {
                saveRanking();
                saveCriteriaSegments();
                saveValueFunctions();
            }
            else
            {
                throw new System.Exception("Results are not available");
            }
        }

        public void saveSession()
        {
            saveInput();
            saveResults();
        }
    }
}
