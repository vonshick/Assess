using DataModel.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace ImportModule
{
    public class UTXLoader : DataLoader
    {
        public UTXLoader() : base()
        {

        }

        private void checkEnumValue(string criterionName, string enumID, string enumValue)
        {
            if (enumValue.Equals(""))
            {
                throw new ImproperFileStructureException("Enum type value can not be empty. Criterion " + criterionName + ", EnumID: " + enumID + ".");
            }

            float output = 0;
            if (!float.TryParse(enumValue, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
            {
                throw new ImproperFileStructureException("Improper value format '" + enumValue + "'. Value has to be floating point. Criterion " + criterionName + ", EnumID: " + enumID + ".");
            }
        }


        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".utx");

            //load XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            string descriptionAttributeName = "";

            // iterate on its nodes
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                if (xmlNode.Name == "ATTRIBUTES")
                {
                    foreach (XmlNode attribute in xmlNode)
                    {
                        Criterion criterion = new Criterion() { };
                        // for UTX ID and Name are the same value
                        criterion.Name = criterion.ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value);
                        bool saveCriterion = true;
                        Dictionary<string, string> enumIdsNamesDictionary = new Dictionary<string, string>();
                        Dictionary<string, float> enumIdsValuesDictionary = new Dictionary<string, float>();

                        foreach (XmlNode attributePart in attribute)
                        {
                            var value = attributePart.Attributes["Value"].Value;

                            switch (attributePart.Name)
                            {
                                case "TYPE":
                                    if (value == "Enum")
                                    {
                                        criterion.IsEnum = true;
                                        foreach (XmlNode enumName in attributePart)
                                        {
                                            enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value, enumName.Attributes["Value"].Value);
                                        }
                                    }
                                    break;
                                case "DESCRIPTION":
                                    criterion.Description = value;
                                    break;
                                case "CRITERION":
                                    // if value is enum type
                                    if (value == "Rank")
                                    {
                                        foreach (XmlNode enumValue in attributePart)
                                        {
                                            checkEnumValue(criterion.Name, enumValue.Attributes["EnumID"].Value, enumValue.Attributes["Value"].Value);
                                            enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value, float.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                        }
                                        criterion.CriterionDirection = "c";
                                    }
                                    else
                                    {
                                        // "Cost" or "Gain"
                                        criterion.CriterionDirection = value == "Cost" ? "c" : "g";
                                    }
                                    break;
                                case "ROLE":
                                    if (value == "Description")
                                    {
                                        saveCriterion = false;
                                        descriptionAttributeName = criterion.Name;
                                    }
                                    else
                                    {
                                        saveCriterion = true;
                                    }
                                    break;
                                case "SEGMENTS":
                                    break;
                                default:
                                    //TODO vonshick warnings
                                    // maybe instead of throwing exception just do nothing
                                    throw new Exception("Attribute " + attributePart.Name + " is not compatible with application.");
                            }
                        }

                        if (saveCriterion)
                        {
                            if (criterion.IsEnum)
                            {
                                criterion.EnumDictionary = new Dictionary<string, float>();
                                foreach (KeyValuePair<string, string> entry in enumIdsNamesDictionary)
                                {
                                    string enumID = entry.Key;
                                    string enumName = entry.Value;
                                    float enumValue = enumIdsValuesDictionary[enumID];
                                    criterion.EnumDictionary.Add(enumName, enumValue);
                                }
                            }
                            criterionList.Add(criterion);
                        }
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    foreach (XmlNode instance in xmlNode)
                    {
                        // check if number of all child nodes (except INFO node - which isn't about criterion) is equal to criterionList.Count
                        int alternativesCountValidation = 0;
                        Alternative alternative = new Alternative() { Name = checkAlternativesNamesUniqueness(instance.Attributes["ObjID"].Value) };
                        alternative.ID = alternative.Name;

                        var criteriaValuesList = new ObservableCollection<CriterionValue>();

                        foreach (XmlNode instancePart in instance)
                        {
                            // we avoid RANK nodes
                            if (instancePart.Name.Equals("VALUE"))
                            {
                                var value = instancePart.Attributes["Value"].Value;
                                var attributeName = instancePart.Attributes["AttrID"].Value;

                                if (attributeName != descriptionAttributeName)
                                {
                                    alternativesCountValidation++;
                                    Criterion criterion = criterionList.Find(element => element.Name == attributeName);

                                    if (criterion == null)
                                    {
                                        throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name + ": criterion named " + attributeName + " does not exist.");
                                    }


                                    if (criterion.IsEnum)
                                    {
                                        criteriaValuesList.Add(new CriterionValue(criterion.Name, criterion.EnumDictionary[value]));
                                    }
                                    else
                                    {
                                        checkIfValueIsValid(value, criterion.Name, alternative.Name);
                                        criteriaValuesList.Add(new CriterionValue(criterion.Name, float.Parse(value, CultureInfo.InvariantCulture)));
                                    }
                                }
                            }
                        }

                        if (alternativesCountValidation != criterionList.Count)
                        {
                            throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name + ": there are provided " + alternativesCountValidation + " criteria values and required are " + criterionList.Count + ".");
                        }

                        alternative.CriteriaValuesList = criteriaValuesList;
                        alternativeList.Add(alternative);
                    }
                }
            }
        }
    }
}