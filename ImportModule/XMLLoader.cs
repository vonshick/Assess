using DataModel.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace ImportModule
{
    public class XMLLoader : DataLoader
    {

        public XMLLoader() : base()
        {
        }

        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".xml");

            //load XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            string nameAttributeId = "";
            string descriptionAttributeId = "";

            // iterate on its nodes
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                // first group of nodes are attributes
                // second - Electre meta data
                // third - objects
                if (xmlNode.Name == "ATTRIBUTES")
                {
                    foreach (XmlNode attribute in xmlNode)
                    {
                        Criterion criterion = new Criterion() { ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value) };
                        // two specific groups of nodes may appear in attributes - name and description
                        // we don't want to save it as criterion
                        bool saveCriterion = true;

                        foreach (XmlNode attributePart in attribute)
                        {
                            var value = attributePart.Attributes["Value"].Value;

                            switch (attributePart.Name)
                            {
                                case "NAME":
                                    criterion.Name = checkCriteriaNamesUniqueness(value);
                                    break;
                                case "DESCRIPTION":
                                    criterion.Description = value;
                                    break;
                                case "CRITERION":
                                    if (value == "Cost" || value == "Gain")
                                    {
                                        criterion.CriterionDirection = value == "Cost" ? "c" : "g";
                                    }
                                    else
                                    {
                                        //TODO
                                        // 'Rank' case 
                                        // to serve it some way 
                                        // probably dialog with user will be necessary
                                        // so far set is as gain
                                        criterion.CriterionDirection = "c";
                                    }

                                    break;
                                case "ROLE":
                                    if (value == "Name")
                                    {
                                        saveCriterion = false;
                                        nameAttributeId = criterion.ID;
                                    }
                                    else if (value == "Description")
                                    {
                                        saveCriterion = false;
                                        descriptionAttributeId = criterion.ID;
                                    }
                                    else
                                    {
                                        saveCriterion = true;
                                    }

                                    break;
                                case "TYPE":
                                    break;
                                default:
                                    Console.WriteLine("Improper XML structure");
                                    return;
                            }
                        }

                        if (saveCriterion)
                        {
                            criterionList.Add(criterion);
                        }
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    int nodeCounter = 1;

                    foreach (XmlNode instance in xmlNode)
                    {

                        Alternative alternative = new Alternative();

                        if ((instance.ChildNodes.Count - 2) != criterionList.Count)
                        {
                            throw new ImproperFileStructureException("There are provided " + (instance.ChildNodes.Count - 2) + " criteria values and required are " + criterionList.Count + ".");
                        }

                        List<CriterionValue> criteriaValuesList = new List<CriterionValue>();

                        foreach (XmlNode instancePart in instance)
                        {
                            var value = instancePart.Attributes["Value"].Value;
                            var attributeID = instancePart.Attributes["AttrID"].Value;

                            if (attributeID == descriptionAttributeId)
                            {
                                alternative.Description = value;
                            }
                            else if (attributeID == nameAttributeId)
                            {
                                alternative.Name = checkAlternativesNamesUniqueness(value);
                            }
                            else
                            {
                                Criterion criterion = criterionList.Find(element => element.ID == attributeID);

                                if (criterion == null)
                                {
                                    throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name + ": criterion with ID " + attributeID + " does not exist.");
                                }

                                checkIfValueIsValid(value, criterion.Name, nodeCounter.ToString());

                                criteriaValuesList.Add(new CriterionValue(criterion.Name, float.Parse(value, CultureInfo.InvariantCulture)));
                            }
                        }

                        alternative.CriteriaValuesList = criteriaValuesList;
                        alternativeList.Add(alternative);
                        nodeCounter++;
                    }
                }
            }
        }
    }
}