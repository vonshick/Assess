using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using DataModel.Structs;

namespace ImportModule
{
    public class XMCDALoader : DataLoader
    {
        private string xmcdaDirectory;
        private string currentlyProcessedFile;
        private string currentlyProcessedAlternativeId;

        public XMCDALoader() : base()
        {
        }

        private void validateInputFilesSet()
        {
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "alternatives.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria_scales.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "performance_table.xml"));
        }

        private bool checkIfResultsAvailable()
        {
            if (!File.Exists(Path.Combine(xmcdaDirectory, "alternatives_ranks.xml")) ||
                    !File.Exists(Path.Combine(xmcdaDirectory, "value_functions.xml")))
                return false;

            return true;
        }

        private void checkIfIdProvided(XmlAttributeCollection attributesCollection, string elementType)
        {
            if (attributesCollection["id"] == null)
                throw new ImproperFileStructureException("Attribute 'id' must be provided for each  " + elementType + ".");
        }

        private string checkIfAlternativeNameProvided(XmlAttributeCollection attributesCollection)
        {
            if (attributesCollection["name"] != null)
            {
                return checkAlternativesNamesUniqueness(attributesCollection["name"].Value);
            }
            else
            {
                return checkAlternativesNamesUniqueness(attributesCollection["id"].Value);
            }
        }

        private string checkIfCriterionNameProvided(XmlAttributeCollection attributesCollection)
        {
            if (attributesCollection["name"] != null)
            {
                return checkCriteriaNamesUniqueness(attributesCollection["name"].Value);
            }
            else
            {
                return checkCriteriaNamesUniqueness(attributesCollection["id"].Value);
            }
        }

        private XmlDocument loadFile(string fileName)
        {
            currentlyProcessedFile = Path.Combine(xmcdaDirectory, fileName);
            ValidateFilePath(currentlyProcessedFile);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(currentlyProcessedFile);

            return xmlDocument;
        }

        private void LoadCriteria()
        {
            XmlDocument xmlDocument = loadFile("criteria.xml");

            // this file contains only one main block - <criteria>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                checkIfIdProvided(xmlNode.Attributes, "criterion");
                Criterion criterion = new Criterion()
                {
                    ID = checkCriteriaIdsUniqueness(xmlNode.Attributes["id"].Value)
                };

                criterion.Name = checkIfCriterionNameProvided(xmlNode.Attributes);

                criterionList.Add(criterion);
            }
        }

        private void LoadCriteriaScales()
        {
            XmlDocument xmlDocument = loadFile("criteria_scales.xml");

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                string criterionID = xmlNode.ChildNodes[0].InnerText;
                string criterionDirection = xmlNode.ChildNodes[1].FirstChild.FirstChild.FirstChild.InnerText;

                var index = criterionList.FindIndex(criterion => criterion.ID == criterionID);
                criterionList[index].CriterionDirection = criterionDirection == "max" ? "Gain" : "Cost";
            }
        }

        private void LoadAlternatives()
        {
            XmlDocument xmlDocument = loadFile("alternatives.xml");

            // this file contains only one main block - <criteria>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                checkIfIdProvided(xmlNode.Attributes, "alternative");
                Alternative alternative = new Alternative()
                {
                    ID = checkAlternativesIdsUniqueness(xmlNode.Attributes["id"].Value),
                    CriteriaValuesList = new ObservableCollection<CriterionValue>()
                };

                alternative.Name = checkIfAlternativeNameProvided(xmlNode.Attributes);

                alternativeList.Add(alternative);
            }
        }

        private bool compareAlternativeIds(Alternative alternative)
        {
            return alternative.ID.Equals(currentlyProcessedAlternativeId);
        }

        private void LoadPerformanceTable()
        {
            XmlDocument xmlDocument = loadFile("performance_table.xml");
            int nodeCounter = 1;

            if (xmlDocument.DocumentElement.ChildNodes[0].ChildNodes.Count != alternativeList.Count)
            {
                throw new ImproperFileStructureException("There are provided " + (xmlDocument.DocumentElement.ChildNodes[0].ChildNodes.Count) + " alternative performances and required are " + alternativeList.Count+ ".");
            }

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                // one of children nodes is the name node  
                if ((xmlNode.ChildNodes.Count - 1) != criterionList.Count)
                {
                    throw new ImproperFileStructureException("There are provided " + (xmlNode.ChildNodes.Count - 1) + " criteria values and required are " + criterionList.Count + ": node " + nodeCounter + " of alternativePerformances.");
                }

                foreach (XmlNode performance in xmlNode.ChildNodes)
                {
                    // first node containts alternative ID
                    if (performance.Name == "alternativeID")
                    {
                        currentlyProcessedAlternativeId = performance.InnerText;
                    }
                    else
                    {
                        string criterionID = performance.ChildNodes[0].InnerText;
                        Criterion matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);

                        if (matchingCriterion == null)
                        {
                            throw new ImproperFileStructureException("Error while processing alternative " + currentlyProcessedAlternativeId + ": criterion with ID " + criterionID + " does not exist.");
                        }

                        string value = performance.ChildNodes[1].FirstChild.InnerText;
                        checkIfValueIsValid(value, criterionID, currentlyProcessedAlternativeId);

                        int alternativeIndex = alternativeList.FindIndex(compareAlternativeIds);

                        alternativeList[alternativeIndex].CriteriaValuesList.Add(new CriterionValue(matchingCriterion.Name, float.Parse(value, CultureInfo.InvariantCulture)));
                    }
                }

                nodeCounter++;
            }
        }

        private void LoadAlternativesRanks()
        {
            XmlDocument xmlDocument = loadFile("alternatives_ranks.xml");

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                foreach (XmlNode alternativeResult in xmlNode.ChildNodes)
                {
                    // first node containts alternative ID
                    if (alternativeResult.Name == "alternativeID")
                    {
                        currentlyProcessedAlternativeId = alternativeResult.InnerText;
                    }
                    else
                    {
                        int rank = int.Parse(alternativeResult.ChildNodes[0].InnerText);
                        int alternativeIndex = alternativeList.FindIndex(compareAlternativeIds);
                        alternativeList[alternativeIndex].ReferenceRank = rank;
                    }
                }
            }
        }

        private void LoadValueFunctions()
        {
            XmlDocument xmlDocument = loadFile("value_functions.xml");

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                string criterionID = "";
                List<PartialUtilityValues> argumentsValues = new List<PartialUtilityValues>();
                foreach (XmlNode criterionFunction in xmlNode.ChildNodes)
                {
                    if (criterionFunction.Name == "criterionID")
                    {
                        criterionID = criterionFunction.InnerText;
                    }
                    else
                    {
                        foreach (XmlNode point in criterionFunction.FirstChild.ChildNodes)
                        {
                            float argument = float.PositiveInfinity;
                            float value = float.PositiveInfinity;

                            foreach (XmlNode coordinate in point.ChildNodes)
                            {

                                if (coordinate.Name == "abscissa")
                                {
                                    argument = float.Parse(coordinate.FirstChild.InnerText, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    value = float.Parse(coordinate.FirstChild.InnerText, CultureInfo.InvariantCulture);
                                    if (argument == float.PositiveInfinity || value == float.PositiveInfinity)
                                    {
                                        Trace.WriteLine("Format of value_functions.xml file is not valid");
                                        return;
                                    }

                                    argumentsValues.Add(new PartialUtilityValues(argument, value));
                                }
                            }
                        }

                        var matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);
                        results.PartialUtilityFunctions.Add(new PartialUtility(matchingCriterion, argumentsValues));
                    }
                }
            }
        }

        override protected void ProcessFile(string xmcdaDirectory)
        {
            this.xmcdaDirectory = xmcdaDirectory;

            validateInputFilesSet();

            LoadCriteria();
            LoadCriteriaScales();
            LoadAlternatives();
            LoadPerformanceTable();
            setMinAndMaxCriterionValues();
            
            if(checkIfResultsAvailable())
            {
                LoadAlternativesRanks();
                LoadValueFunctions();
            }
        }
    }
}
