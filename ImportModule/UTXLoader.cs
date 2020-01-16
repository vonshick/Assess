using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class UTXLoader : DataLoader
    {
        private void checkEnumValue(string criterionName, string enumID, string enumValue)
        {
            if (enumValue.Equals(""))
                throw new ImproperFileStructureException("Enum type value can not be empty. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");

            double output = 0;
            if (!double.TryParse(enumValue, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
                throw new ImproperFileStructureException("Improper value format '" + enumValue +
                                                         "'. Value has to be floating point. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");
        }

        private Criterion validateCriterion(Criterion criterion, Dictionary<string, string> enumNames,
            Dictionary<string, double> enumValues)
        {
            if (criterion.IsEnum)
            {
                criterion.EnumDictionary = new Dictionary<string, double>();
                foreach (var entry in enumNames)
                {
                    var enumID = entry.Key;
                    var enumName = entry.Value;
                    var enumValue = enumValues[enumID];
                    criterion.EnumDictionary.Add(enumName, enumValue);
                }
            }

            if (criterion.Name == null || criterion.Name.Equals(""))
                criterion.Name = criterion.ID;

            if (criterion.CriterionDirection == null || criterion.CriterionDirection.Equals(""))
                throw new ImproperFileStructureException("There was no criterion scale ('Cost' or 'Gain') provided for criterion " +
                                                         criterion.Name);

            return criterion;
        }

        protected override void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".utx");

            //load XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            //var nameAttributeId = "";
            var descriptionAttributeName = "";

            var attributes = xmlDoc.GetElementsByTagName("ATTRIBUTES");

            var objects = xmlDoc.GetElementsByTagName("OBJECTS");

            // iterate on its nodes
            foreach (XmlNode attribute in attributes[0].ChildNodes)
            {
                var criterion = new Criterion {LinearSegments = 1};
                // for UTX ID and Name are the same value
                criterion.Name = criterion.ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value);
                var saveCriterion = true;
                var enumIdsNamesDictionary = new Dictionary<string, string>();
                var enumIdsValuesDictionary = new Dictionary<string, double>();

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
                                    enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value,
                                        enumName.Attributes["Value"].Value);
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
                                    checkEnumValue(criterion.Name, enumValue.Attributes["EnumID"].Value,
                                        enumValue.Attributes["Value"].Value);
                                    enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value,
                                        double.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                }

                                criterion.CriterionDirection = "Cost";
                            }
                            else
                            {
                                // "Cost" or "Gain"
                                criterion.CriterionDirection = value == "Cost" ? "Cost" : "Gain";
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
                    criterion = validateCriterion(criterion, enumIdsNamesDictionary, enumIdsValuesDictionary);
                    criterionList.Add(criterion);
                }
            }


            foreach (XmlNode instance in objects[0].ChildNodes)
            {
                // check if number of all child nodes (except INFO node - which isn't about criterion) is equal to criterionList.Count
                var alternativesCountValidation = 0;
                var alternative = new Alternative {Name = checkAlternativesNamesUniqueness(instance.Attributes["ObjID"].Value)};
                alternative.ID = alternative.Name;

                var criteriaValuesList = new ObservableCollection<CriterionValue>();

                foreach (XmlNode instancePart in instance)
                    // we avoid RANK nodes
                    if (instancePart.Name.Equals("VALUE"))
                    {
                        var value = instancePart.Attributes["Value"].Value;
                        var attributeName = instancePart.Attributes["AttrID"].Value;

                        if (attributeName != descriptionAttributeName)
                        {
                            alternativesCountValidation++;
                            var criterion = criterionList.Find(element => element.Name == attributeName);

                            if (criterion == null)
                                throw new ImproperFileStructureException(
                                    "Error while processing alternative " + alternative.Name + ": criterion named " +
                                    attributeName + " does not exist.");


                            if (criterion.IsEnum)
                            {
                                criteriaValuesList.Add(new CriterionValue(criterion.Name, criterion.EnumDictionary[value]));
                            }
                            else
                            {
                                checkIfValueIsValid(value, criterion.Name, alternative.Name);
                                criteriaValuesList.Add(new CriterionValue(criterion.Name,
                                    double.Parse(value, CultureInfo.InvariantCulture)));
                            }
                        }
                    }

                if (alternativesCountValidation != criterionList.Count)
                    throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name +
                                                             ": there are provided " + alternativesCountValidation +
                                                             " criteria values and required are " + criterionList.Count + ".");

                alternative.CriteriaValuesList = criteriaValuesList;
                alternativeList.Add(alternative);
            }
        }
    }
}