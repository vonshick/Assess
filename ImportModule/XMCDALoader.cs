using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using DataModel.Input;
using DataModel.Results;

namespace ImportModule
{
    public class XMCDALoader : DataLoader
    {
        private string currentlyProcessedAlternativeId;
        private string currentlyProcessedFile;
        private string xmcdaDirectory;

        private void validateInputFilesSet()
        {
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "alternatives.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria_scales.xml"));
            ValidateFilePath(Path.Combine(xmcdaDirectory, "performance_table.xml"));
        }

        private void checkIfIdProvided(XmlAttributeCollection attributesCollection, string elementType)
        {
            if (attributesCollection["id"] == null)
                throw new ImproperFileStructureException("Attribute 'id' must be provided for each  " + elementType + ".");
        }

        private string checkIfAlternativeNameProvided(XmlAttributeCollection attributesCollection)
        {
            if (attributesCollection["name"] != null)
                return checkAlternativesNamesUniqueness(attributesCollection["name"].Value);
            return checkAlternativesNamesUniqueness(attributesCollection["id"].Value);
        }

        private string checkIfCriterionNameProvided(XmlAttributeCollection attributesCollection)
        {
            if (attributesCollection["name"] != null)
                return checkCriteriaNamesUniqueness(attributesCollection["name"].Value);
            return checkCriteriaNamesUniqueness(attributesCollection["id"].Value);
        }

        private XmlDocument loadFile(string fileName)
        {
            currentlyProcessedFile = Path.Combine(xmcdaDirectory, fileName);
            ValidateFilePath(currentlyProcessedFile);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(currentlyProcessedFile);

            return xmlDocument;
        }

        private void LoadCriteria()
        {
            var xmlDocument = loadFile("criteria.xml");

            // this file contains only one main block - <criteria>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                checkIfIdProvided(xmlNode.Attributes, "criterion");
                var criterion = new Criterion
                {
                    ID = checkCriteriaIdsUniqueness(xmlNode.Attributes["id"].Value)
                };

                criterion.Name = checkIfCriterionNameProvided(xmlNode.Attributes);

                criterionList.Add(criterion);
            }
        }

        private void LoadCriteriaScales()
        {
            var xmlDocument = loadFile("criteria_scales.xml");

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                var criterionID = xmlNode.ChildNodes[0].InnerText;
                var criterionDirection = xmlNode.ChildNodes[1].FirstChild.FirstChild.FirstChild.InnerText;

                var index = criterionList.FindIndex(criterion => criterion.ID == criterionID);
                criterionList[index].CriterionDirection = criterionDirection == "max" ? "Gain" : "Cost";
            }
        }

        private void LoadAlternatives()
        {
            var xmlDocument = loadFile("alternatives.xml");

            // this file contains only one main block - <criteria>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                checkIfIdProvided(xmlNode.Attributes, "alternative");
                var alternative = new Alternative
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
            var xmlDocument = loadFile("performance_table.xml");
            var nodeCounter = 1;

            if (xmlDocument.DocumentElement.ChildNodes[0].ChildNodes.Count != alternativeList.Count)
                throw new ImproperFileStructureException("There are provided " +
                                                         xmlDocument.DocumentElement.ChildNodes[0].ChildNodes.Count +
                                                         " alternative performances and required are " + alternativeList.Count + ".");

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                // one of children nodes is the name node  
                if (xmlNode.ChildNodes.Count - 1 != criterionList.Count)
                    throw new ImproperFileStructureException("There are provided " + (xmlNode.ChildNodes.Count - 1) +
                                                             " criteria values and required are " + criterionList.Count + ": node " +
                                                             nodeCounter + " of alternativePerformances.");

                foreach (XmlNode performance in xmlNode.ChildNodes)
                    // first node containts alternative ID
                    if (performance.Name == "alternativeID")
                    {
                        currentlyProcessedAlternativeId = performance.InnerText;
                    }
                    else
                    {
                        var criterionID = performance.ChildNodes[0].InnerText;
                        var matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);

                        if (matchingCriterion == null)
                            throw new ImproperFileStructureException("Error while processing alternative " +
                                                                     currentlyProcessedAlternativeId + ": criterion with ID " +
                                                                     criterionID + " does not exist.");

                        var value = performance.ChildNodes[1].FirstChild.InnerText;
                        checkIfValueIsValid(value, criterionID, currentlyProcessedAlternativeId);

                        var alternativeIndex = alternativeList.FindIndex(compareAlternativeIds);

                        alternativeList[alternativeIndex].CriteriaValuesList.Add(new CriterionValue(matchingCriterion.Name,
                            double.Parse(value, CultureInfo.InvariantCulture)));
                    }

                nodeCounter++;
            }
        }

        private string GetDialogMethod(XmlNode xmlNode)
        {
            if (xmlNode.Attributes["concept"] != null)
            {
                switch (xmlNode.Attributes["concept"].Value)
                {
                    case "constantProbability":
                        return Criterion.MethodOptionsList[1];
                    case "variableProbability":
                        return Criterion.MethodOptionsList[2];
                    case "lotteriesComparison":
                        return Criterion.MethodOptionsList[3];
                    case "probabilityComparison":
                        return Criterion.MethodOptionsList[4];
                    default:
                        return Criterion.MethodOptionsList[0];
                }
            }
            return Criterion.MethodOptionsList[0];
        }

        private void LoadValueFunctions()
        {
            currentlyProcessedFile = Path.Combine(xmcdaDirectory, "value_functions.xml");

            if (!File.Exists(currentlyProcessedFile))
                return;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(currentlyProcessedFile);

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                var criterionID = "";
                var argumentsValues = new List<PartialUtilityValues>();
                foreach (XmlNode criterionFunction in xmlNode.ChildNodes)
                    if (criterionFunction.Name == "criterionID")
                    {
                        criterionID = criterionFunction.InnerText;
                    }
                    else
                    {
                        var dialogMethod = GetDialogMethod(criterionFunction);

                        foreach (XmlNode point in criterionFunction.FirstChild.ChildNodes)
                        {
                            var argument = double.PositiveInfinity;
                            var value = double.PositiveInfinity;

                            foreach (XmlNode coordinate in point.ChildNodes)
                                if (coordinate.Name == "abscissa")
                                {
                                    argument = double.Parse(coordinate.FirstChild.InnerText, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    value = double.Parse(coordinate.FirstChild.InnerText, CultureInfo.InvariantCulture);
                                    if (argument == double.PositiveInfinity || value == double.PositiveInfinity)
                                    {
                                        Trace.WriteLine("Format of value_functions.xml file is not valid");
                                        return;
                                    }

                                    argumentsValues.Add(new PartialUtilityValues(argument, value));
                                }
                        }

                        var matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);
                        matchingCriterion.Method = dialogMethod;
                        results.PartialUtilityFunctions.Add(new PartialUtility(matchingCriterion, argumentsValues));
                    }
            }
        }

        private void LoadWeights()
        {
            currentlyProcessedFile = Path.Combine(xmcdaDirectory, "weights.xml");

            if (!File.Exists(currentlyProcessedFile))
                return;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(currentlyProcessedFile);

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                var criterionID = xmlNode.ChildNodes[0].InnerText;

                double coefficient = -1;

                if (xmlNode.ChildNodes[1].FirstChild.FirstChild != null)
                {
                    if (!double.TryParse(xmlNode.ChildNodes[1].FirstChild.FirstChild.InnerText, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out coefficient))
                        throw new ImproperFileStructureException("Improper criterion coefficient format " +
                                                                 xmlNode.ChildNodes[1].FirstChild.FirstChild.InnerText +
                                                                 " - it should be floating point.");
                }
                else
                {
                    throw new ImproperFileStructureException(
                        "Improper structure of weights.xml file. Please compare it to the documentation.");
                }

                var index = criterionList.FindIndex(criterion => criterion.ID == criterionID);
                var criterionName = criterionList.Find(o => o.ID.Equals(criterionID)).Name;
                results.CriteriaCoefficients.Add(new CriterionCoefficient(criterionName, coefficient));
            }
        }

        protected override void ProcessFile(string xmcdaDirectory)
        {
            this.xmcdaDirectory = xmcdaDirectory;

            validateInputFilesSet();

            LoadCriteria();
            LoadCriteriaScales();
            LoadAlternatives();
            LoadPerformanceTable();

            LoadValueFunctions();
            LoadWeights();

            setMinAndMaxCriterionValues();
        }
    }
}