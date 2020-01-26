// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
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
        public string CurrentlyProcessedFile;
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
            CurrentlyProcessedFile = Path.Combine(xmcdaDirectory, fileName);
            ValidateFilePath(CurrentlyProcessedFile);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(CurrentlyProcessedFile);

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

        private void CheckFunctionMonotonicity(PartialUtility partialUtility)
        {
            var criterionId = partialUtility.Criterion.ID;
            var criterionDirection = partialUtility.Criterion.CriterionDirection;
            for (var i = 1; i < partialUtility.PointsValues.Count; i++)
            {
                if (criterionDirection.Equals("Gain"))
                {
                    if (partialUtility.PointsValues[i].Y < partialUtility.PointsValues[i - 1].Y)
                        throw new ImproperFileStructureException("criterion " + criterionId + " - Utility function has to be increasing for criterion direction '" + criterionDirection + "'.");
                }
                else if (criterionDirection.Equals("Cost"))
                {
                    if (partialUtility.PointsValues[i].Y > partialUtility.PointsValues[i - 1].Y)
                        throw new ImproperFileStructureException("criterion " + criterionId + " - Utility function has to be descending  for criterion direction '" + criterionDirection + "'.");

                }
            }
        }

        private void CheckEdgePoints(PartialUtility partialUtility)
        {
            var criterionId = partialUtility.Criterion.ID;
            var criterionDirection = partialUtility.Criterion.CriterionDirection;
            var criterionMax = partialUtility.Criterion.MaxValue;
            var criterionMin = partialUtility.Criterion.MinValue;

            var lowestAbscissa = partialUtility.PointsValues[0].X;
            var highestAbscissa = partialUtility.PointsValues[partialUtility.PointsValues.Count - 1].X;

            var lowestAbscissaUtility = partialUtility.PointsValues[0].Y;
            var highestAbscissaUtility = partialUtility.PointsValues[partialUtility.PointsValues.Count - 1].Y;

            if(lowestAbscissa != criterionMin)
                throw new ImproperFileStructureException("criterion " + criterionId +
                                                             " - lowest abscissa equals "  + lowestAbscissa.ToString("G", CultureInfo.InvariantCulture) +
                                                             " and it should be the same like the lowest value for this criterion in performance_table.xml: " +
                                                             criterionMin.ToString("G", CultureInfo.InvariantCulture) + ".");

            if (lowestAbscissa != criterionMin)
                throw new ImproperFileStructureException("criterion " + criterionId +
                                                         " - lowest abscissa equals " + lowestAbscissa.ToString("G", CultureInfo.InvariantCulture) +
                                                         " and it should be the same like the lowest value for this criterion in performance_table.xml: " +
                                                         criterionMin.ToString("G", CultureInfo.InvariantCulture) + ".");

            if (highestAbscissa != criterionMax)
                throw new ImproperFileStructureException("criterion " + criterionId +
                                                         " - highest abscissa equals " + lowestAbscissa.ToString("G", CultureInfo.InvariantCulture) +
                                                         " and it should be the same like the highest value for this criterion performance_table.xml: " +
                                                         criterionMax.ToString("G", CultureInfo.InvariantCulture) + ".");

            if (criterionDirection.Equals("Gain"))
            {
                if (lowestAbscissaUtility != 0)
                    throw new ImproperFileStructureException("criterion " + criterionId +
                                                             " - Lowest utility value of each function should be equal to 0 and it is " +
                                                             lowestAbscissaUtility.ToString("G", CultureInfo.InvariantCulture) + ".");
                if (highestAbscissaUtility != 1)
                    throw new ImproperFileStructureException("criterion " + criterionId +
                                                             " - Highest utility value of each function should be equal to 1 and it is " +
                                                             highestAbscissaUtility.ToString("G", CultureInfo.InvariantCulture) + ".");
            }
            else if (criterionDirection.Equals("Cost"))
            {
                if (lowestAbscissaUtility != 1)
                    throw new ImproperFileStructureException("criterion " + criterionId +
                                                             " - Highest utility value of each function should be equal to 1 and it is " +
                                                             lowestAbscissaUtility.ToString("G", CultureInfo.InvariantCulture) + ".");
                if (highestAbscissaUtility != 0)
                    throw new ImproperFileStructureException("criterion " + criterionId +
                                                             " - Lowest utility value of each function should be equal to 0 and it is " +
                                                             highestAbscissaUtility.ToString("G", CultureInfo.InvariantCulture) + ".");
            }
        }

        private void ValidateUtilityFunctions()
        {
            foreach (var utilityFunction in results.PartialUtilityFunctions)
            {
                utilityFunction.PointsValues.Sort((first, second) => first.X.CompareTo(second.X));
                CheckEdgePoints(utilityFunction);
                CheckFunctionMonotonicity(utilityFunction);
            }
        }

        private void LoadValueFunctions()
        {
            CurrentlyProcessedFile = Path.Combine(xmcdaDirectory, "value_functions.xml");

            if (!File.Exists(CurrentlyProcessedFile))
                return;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(CurrentlyProcessedFile);

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
                        results.PartialUtilityFunctions.Add(new PartialUtility(matchingCriterion, argumentsValues));
                    }
            }

            ValidateUtilityFunctions();
        }

        private void LoadWeights()
        {
            CurrentlyProcessedFile = Path.Combine(xmcdaDirectory, "weights.xml");

            if (!File.Exists(CurrentlyProcessedFile))
                return;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(CurrentlyProcessedFile);

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

        private string GetDialogMethod(string method)
        {
            switch (method)
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

        private void LoadDialogMethods()
        {
            CurrentlyProcessedFile = Path.Combine(xmcdaDirectory, "method_parameters.xml");

            if (!File.Exists(CurrentlyProcessedFile))
                return;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(CurrentlyProcessedFile);

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                var criterionID = xmlNode.Attributes["id"].Value;

                double probability = -1;
                string method = "";

                if (xmlNode.SelectSingleNode(".//label") != null)
                {
                    method = xmlNode.SelectSingleNode(".//label").InnerText;

                    if (method.Equals("constantProbability") || method.Equals("lotteriesComparison"))
                    {
                        if (xmlNode.SelectSingleNode(".//real") != null)
                        {
                            if (!double.TryParse(xmlNode.SelectSingleNode(".//real").InnerText, NumberStyles.Any,
                                CultureInfo.InvariantCulture, out probability))
                                throw new ImproperFileStructureException("criterion " + criterionID + " - Improper criterion coefficient format " +
                                                                         xmlNode.SelectSingleNode(".//real").InnerText +
                                                                         " - it should be floating point.");
                        }
                    }
                }
                else
                {
                    throw new ImproperFileStructureException(
                        "Improper structure of method_parameters.xml file. Please compare it to the documentation.");
                }

                var matchingCriterion = criterionList.Find(criterion => criterion.ID == criterionID);
                matchingCriterion.Probability = probability;
                matchingCriterion.Method = GetDialogMethod(method);
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
            setMinAndMaxCriterionValues();

            LoadValueFunctions();
            LoadWeights();
            LoadDialogMethods();
            CurrentlyProcessedFile = "";
        }
    }
}