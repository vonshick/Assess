using DataModel.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace ImportModule
{
    public class XMLLoader : DataLoader
    {

        public XMLLoader() : base()
        {
        }

        private string checkCriteriaIdsUniqueness(string id) 
        {
            string[] usedIds = criterionList.Select(criterion => criterion.ID).ToArray();
            foreach(string usedId in usedIds) 
            {
                if(id.Equals(usedId))
                {
                    throw new ImproperFileStructureException("Attribute ID '" + id + "' has been already used!");
                }
            }
            
            return id;
        }

        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);

            try
            {
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
                        foreach (XmlNode instance in xmlNode)
                        {

                            Alternative alternative = new Alternative();
                            Dictionary<Criterion, float> criteriaValuesDictionary = new Dictionary<Criterion, float>();

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
                                    criteriaValuesDictionary.Add(criterion, float.Parse(value, CultureInfo.InvariantCulture));
                                }
                            }

                            alternative.CriteriaValues = criteriaValuesDictionary;
                            alternativeList.Add(alternative);
                        }
                    }
                }
            } catch(Exception exception) {
                if (exception is ImproperFileStructureException)
                {
                    Trace.WriteLine(exception.Message);
                }
                else
                {
                    Trace.WriteLine("Loading XML " + filePath + " failed! " + exception.Message);
                }
            }
        }
    }
}