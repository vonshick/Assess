using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DataModel.Input;
using DataModel.Results;

namespace ExportModule
{
    public class XMCDAExporter
    {
        private readonly List<Alternative> alternativeList;
        private readonly List<Criterion> criterionList;
        private readonly string outputDirectory;
        public bool OverwriteFile;
        private readonly Results results;
        private XmlTextWriter xmcdaWriter;

        public XMCDAExporter(string outputDirectory,
            List<Criterion> criterionList,
            List<Alternative> alternativeList,
            Results results)
        {
            OverwriteFile = false;
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
            this.results = results;
        }

        public XMCDAExporter(string outputDirectory,
            List<Criterion> criterionList,
            List<Alternative> alternativeList)
        {
            OverwriteFile = false;
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
        }

        private void checkIfFileExists(string path)
        {
            if (!OverwriteFile)
                if (File.Exists(path))
                    throw new XmcdaFileExistsException(
                        "File " + Path.GetFileName(path) + " already exists. Would you like to overwrite it?");
        }

        private void checkIfInputFilesExists()
        {
            checkIfFileExists(Path.Combine(outputDirectory, "criteria.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "alternatives.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "performance_table.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "criteria_scales.xml"));
        }

        private void checkIfResultFilesExists()
        {
            checkIfFileExists(Path.Combine(outputDirectory, "alternatives_ranks.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "criteria_segments.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "value_functions.xml"));
        }

        private void initializeWriter(string filePath)
        {
            xmcdaWriter = new XmlTextWriter(filePath, Encoding.UTF8);
            xmcdaWriter.Formatting = Formatting.Indented;
            xmcdaWriter.Indentation = 2;
            xmcdaWriter.WriteStartDocument(false);
            xmcdaWriter.WriteStartElement("xmcda:XMCDA");
            xmcdaWriter.WriteAttributeString("xmlns:xmcda", "http://www.decision-deck.org/2016/XMCDA-3.0.2");
            xmcdaWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xmcdaWriter.WriteAttributeString("xsi:schemaLocation",
                "http://www.decision-deck.org/2016/XMCDA-3.0.2 http://www.decision-deck.org/xmcda/_downloads/XMCDA-3.0.2.xsd");
        }

        private void saveCriterions()
        {
            initializeWriter(Path.Combine(outputDirectory, "criteria.xml"));
            xmcdaWriter.WriteStartElement("criteria");

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteAttributeString("id", criterion.ID != null ? criterion.ID : criterion.Name);
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

        private void saveAlternatives()
        {
            initializeWriter(Path.Combine(outputDirectory, "alternatives.xml"));
            xmcdaWriter.WriteStartElement("alternatives");

            foreach (var alternative in alternativeList)
            {
                xmcdaWriter.WriteStartElement("alternative");
                xmcdaWriter.WriteAttributeString("id", alternative.ID != null ? alternative.ID : alternative.Name);
                xmcdaWriter.WriteAttributeString("name", alternative.Name);
                xmcdaWriter.WriteStartElement("type");
                xmcdaWriter.WriteString("real");
                xmcdaWriter.WriteEndElement();
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

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterionScale");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(criterion.ID != null ? criterion.ID : criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("scales");
                xmcdaWriter.WriteStartElement("scale");
                xmcdaWriter.WriteStartElement("quantitative");
                xmcdaWriter.WriteStartElement("preferenceDirection");
                xmcdaWriter.WriteString(criterion.CriterionDirection == "Cost" ? "min" : "max");
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
            foreach (var alternative in alternativeList)
            {
                xmcdaWriter.WriteStartElement("alternativePerformances");
                xmcdaWriter.WriteStartElement("alternativeID");
                xmcdaWriter.WriteString(alternative.ID != null ? alternative.ID : alternative.Name);
                xmcdaWriter.WriteEndElement();

                foreach (var criterionValue in alternative.CriteriaValuesList)
                {
                    xmcdaWriter.WriteStartElement("performance");
                    xmcdaWriter.WriteStartElement("criterionID");
                    var matchingCriterion = criterionList.Find(criterion => criterion.Name == criterionValue.Name);
                    xmcdaWriter.WriteString(matchingCriterion.ID != null ? matchingCriterion.ID : matchingCriterion.Name);
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteStartElement("values");
                    xmcdaWriter.WriteStartElement("value");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(((double) criterionValue.Value).ToString("G", CultureInfo.InvariantCulture));
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

        private void saveReferenceRanking()
        {
            initializeWriter(Path.Combine(outputDirectory, "alternatives_ranks.xml"));
            xmcdaWriter.WriteStartElement("alternativesValues");

            foreach (var alternative in alternativeList)
                if (alternative.ReferenceRank != null)
                {
                    xmcdaWriter.WriteStartElement("alternativeValue");
                    xmcdaWriter.WriteStartElement("alternativeID");
                    xmcdaWriter.WriteString(alternative.ID != null ? alternative.ID : alternative.Name);
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteStartElement("value");
                    xmcdaWriter.WriteStartElement("integer");
                    xmcdaWriter.WriteString(alternative.ReferenceRank.ToString());
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

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterionValue");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(criterion.ID != null ? criterion.ID : criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("value");
                xmcdaWriter.WriteStartElement("integer");
                xmcdaWriter.WriteString(criterion.LinearSegments.ToString());
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

            foreach (var partialUtility in results.PartialUtilityFunctions)
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(partialUtility.Criterion.ID != null ? partialUtility.Criterion.ID : partialUtility.Criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("criterionFunction");
                xmcdaWriter.WriteStartElement("points");
                foreach (var pointValue in partialUtility.PointsValues)
                {
                    xmcdaWriter.WriteStartElement("point");

                    xmcdaWriter.WriteStartElement("abscissa");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.X.ToString("G", CultureInfo.InvariantCulture));
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();

                    xmcdaWriter.WriteStartElement("ordinate");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.Y.ToString("G", CultureInfo.InvariantCulture));
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
            checkIfInputFilesExists();
            saveCriterions();
            saveAlternatives();
            saveCriterionScales();
            savePerformanceTable();
        }

        public void saveResults()
        {
            checkIfResultFilesExists();
            if (results != null)
            {
                saveReferenceRanking();
                saveCriteriaSegments();
                saveValueFunctions();
            }
            else
            {
                throw new Exception("Results are not available");
            }
        }

        public void saveSession()
        {
            saveInput();
            saveResults();
        }
    }
}