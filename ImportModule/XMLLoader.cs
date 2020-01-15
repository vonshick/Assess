using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class XMLLoader : DataLoader
    {
        protected override void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".xml");

            //load XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var nameAttributeId = "";
            var descriptionAttributeId = "";

            // iterate on its nodes
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
                // first group of nodes are attributes
                // second - Electre meta data
                // third - objects
                if (xmlNode.Name == "ATTRIBUTES")
                {
                    foreach (XmlNode attribute in xmlNode)
                    {
                        var criterion = new Criterion
                            {ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value), LinearSegments = 1};
                        // two specific groups of nodes may appear in attributes - name and description
                        // we don't want to save it as criterion
                        var saveCriterion = true;

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
                                        criterion.CriterionDirection = value == "Cost" ? "Cost" : "Gain";
                                    else
                                        //TODO
                                        // 'Rank' case 
                                        // to serve it some way 
                                        // probably dialog with user will be necessary
                                        // so far set is as gain
                                        criterion.CriterionDirection = "Cost";

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

                        if (saveCriterion) criterionList.Add(criterion);
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    var nodeCounter = 1;

                    foreach (XmlNode instance in xmlNode)
                    {
                        var alternative = new Alternative();
                        alternative.ID = checkAlternativesIdsUniqueness(instance.Attributes["ObjID"].Value);

                        if (instance.ChildNodes.Count - 2 != criterionList.Count)
                            throw new ImproperFileStructureException("There are provided " + (instance.ChildNodes.Count - 2) +
                                                                     " criteria values and required are " + criterionList.Count + ".");

                        var criteriaValuesList = new ObservableCollection<CriterionValue>();

                        foreach (XmlNode instancePart in instance)
                        {
                            var value = instancePart.Attributes["Value"].Value;
                            var attributeID = instancePart.Attributes["AttrID"].Value;

                            if (attributeID == nameAttributeId)
                            {
                                alternative.Name = checkAlternativesNamesUniqueness(value);
                            }
                            else
                            {
                                if (attributeID != descriptionAttributeId)
                                {
                                    var criterion = criterionList.Find(element => element.ID == attributeID);

                                    if (criterion == null)
                                        throw new ImproperFileStructureException(
                                            "Error while processing alternative " + alternative.Name + ": criterion with ID " +
                                            attributeID + " does not exist.");

                                    checkIfValueIsValid(value, criterion.Name, nodeCounter.ToString());

                                    criteriaValuesList.Add(new CriterionValue(criterion.Name,
                                        double.Parse(value, CultureInfo.InvariantCulture)));
                                }
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