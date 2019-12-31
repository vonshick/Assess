using DataModel.Input;
using DataModel.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace ImportModule
{
    public class XMCDALoader : DataLoader
    {
        private string xmcdaDirectory;
        private string currentlyProcessedFile;

        private List<KeyValuePair<Alternative, int>> alternativesRanking;
        private List<PartialUtility> partialUtilityList;

        public XMCDALoader() : base()
        {
            alternativesRanking = new List<KeyValuePair<Alternative, int>>();
            partialUtilityList = new List<PartialUtility>();
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
                Criterion criterion = new Criterion()
                {
                    Name = checkCriteriaNamesUniqueness(xmlNode.Attributes["name"].Value),
                    ID = checkCriteriaIdsUniqueness(xmlNode.Attributes["id"].Value)
                };

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
                criterionList[index].CriterionDirection = criterionDirection == "max" ? "g" : "c";
            }
        }

        private void LoadPerformanceTable()
        {
            XmlDocument xmlDocument = loadFile("performance_table.xml");
            int nodeCounter = 1;

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                Alternative alternative = new Alternative { CriteriaValues = new Dictionary<Criterion, float>() };

                // one of children nodes is the name node  
                if ((xmlNode.ChildNodes.Count - 1) != criterionList.Count)
                {
                    throw new ImproperFileStructureException("There are provided " + (xmlNode.ChildNodes.Count - 1) + " criteria values and required are " + criterionList.Count + ": node " + nodeCounter + " of alternativePerformances");
                }

                foreach (XmlNode performance in xmlNode.ChildNodes)
                {
                    // first node containts alternative ID
                    if (performance.Name == "alternativeID")
                    {
                        alternative.Name = checkAlternativesNamesUniqueness(performance.InnerText);
                    }
                    else
                    {
                        string criterionID = performance.ChildNodes[0].InnerText;
                        Criterion matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);

                        if (matchingCriterion == null)
                        {
                            throw new ImproperFileStructureException(alternative.Name + ": Criterion with ID " + criterionID + " does not exist");
                        }

                        string value = performance.ChildNodes[1].FirstChild.InnerText;
                        checkIfValueIsValid(value, criterionID, alternative.Name);

                        alternative.CriteriaValuesList.Add(new CriterionValue(matchingCriterion.Name, float.Parse(value, CultureInfo.InvariantCulture)));
                    }
                }

                alternativeList.Add(alternative);
                nodeCounter++;
            }
        }

        private void LoadAlternativesRanks()
        {
            XmlDocument xmlDocument = loadFile("alternatives_ranks.xml");

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                string alternativeName = "";

                foreach (XmlNode alternativeResult in xmlNode.ChildNodes)
                {
                    // first node containts alternative ID
                    if (alternativeResult.Name == "alternativeID")
                    {
                        alternativeName = alternativeResult.InnerText;
                    }
                    else
                    {
                        int rank = int.Parse(alternativeResult.ChildNodes[0].InnerText);
                        Alternative matchingAlternative = alternativeList.Find(alternative => alternative.Name == alternativeName);
                        alternativesRanking.Add(new KeyValuePair<Alternative, int>(matchingAlternative, rank));
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
                                    argument = float.Parse(coordinate.FirstChild.InnerText);
                                }
                                else
                                {
                                    value = float.Parse(coordinate.FirstChild.InnerText);
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
                        partialUtilityList.Add(new PartialUtility(matchingCriterion, argumentsValues));
                    }
                }
            }
        }

        override protected void ProcessFile(string xmcdaDirectory)
        {
            this.xmcdaDirectory = xmcdaDirectory;
            try
            {
                LoadCriteria();
                LoadCriteriaScales();
                LoadPerformanceTable();
                setMinAndMaxCriterionValues();
            }
            catch (Exception exception)
            {
                if (exception is ImproperFileStructureException)
                {
                    Trace.WriteLine(exception.Message);
                }
                else
                {
                    Trace.WriteLine("Loading XML " + currentlyProcessedFile + " failed! " + exception.Message);
                }
            }
        }

        public void LoadResults()
        {
            LoadAlternativesRanks();
            LoadValueFunctions();
        }

    }
}
