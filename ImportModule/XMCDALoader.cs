using DataModel.Input;
using DataModel.Results;
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

        private List<KeyValuePair<Alternative, int>> alternativesRanking;
        private List<PartialUtility> partialUtilityList;

        public XMCDALoader() : base()
        {
            alternativesRanking = new List<KeyValuePair<Alternative, int>>();
            partialUtilityList = new List<PartialUtility>();
        }

        private void LoadCriteria()
        {
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria.xml"));

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(xmcdaDirectory, "criteria.xml"));

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
            ValidateFilePath(Path.Combine(xmcdaDirectory, "criteria_scales.xml"));

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(xmcdaDirectory, "criteria_scales.xml"));

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
            ValidateFilePath(Path.Combine(xmcdaDirectory, "performance_table.xml"));

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(xmcdaDirectory, "performance_table.xml"));

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                Alternative alternative = new Alternative { CriteriaValues = new Dictionary<Criterion, float>() };

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
                        float value = float.Parse(performance.ChildNodes[1].FirstChild.InnerText, CultureInfo.InvariantCulture);
                        alternative.CriteriaValuesList.Add(new CriterionValue(matchingCriterion.Name, value));
                    }
                }

                alternativeList.Add(alternative);
            }
        }

        private void LoadAlternativesRanks()
        {
            ValidateFilePath(Path.Combine(xmcdaDirectory, "alternatives_ranks.xml"));

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(xmcdaDirectory, "alternatives_ranks.xml"));

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
            ValidateFilePath(Path.Combine(xmcdaDirectory, "value_functions.xml"));

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(xmcdaDirectory, "value_functions.xml"));

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                string criterionID = "";
                Dictionary<float, float> argumentsValues = new Dictionary<float, float>();

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

                                    argumentsValues.Add(argument, value);
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
            LoadCriteria();
            LoadCriteriaScales();
            LoadPerformanceTable();
            setMinAndMaxCriterionValues();
        }

        public void LoadResults()
        {
            LoadAlternativesRanks();
            LoadValueFunctions();
        }

    }
}
