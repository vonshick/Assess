using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class DataLoader
    {
        public List<Criterion> CriterionList { get; set; }
        public List<Alternative> AlternativeList { get; set; }

        public void LoadCSV(string filePath)
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();

            using (var reader = new StreamReader(filePath))
            {
                string[] criterionDirectionsArray = reader.ReadLine().Split(';');
                string[] criterionNamesArray = reader.ReadLine().Split(';');
                
                // iterating from 1 because first column is empty
                for (int i = 1; i < criterionDirectionsArray.Length; i++)
                {
                    CriterionList.Add(new Criterion(criterionNamesArray[i], criterionDirectionsArray[i]));
                }

                while (!reader.EndOfStream)
                {
                    var values = reader.ReadLine().Split(';');
                    Alternative alternative = new Alternative {Name = values[0]};
                    Dictionary<string, float> criterionValueDictionary = new Dictionary<string, float>();

                    for (int i = 1; i < values.Length; i++)
                    {
                        criterionValueDictionary.Add(criterionNamesArray[i], float.Parse(values[i], CultureInfo.InvariantCulture));
                    }

                    alternative.CriteriaValues = criterionValueDictionary;
                    AlternativeList.Add(alternative);
                }
            }
        }

        public void LoadXML(string filePath)
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();

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
                        Criterion criterion = new Criterion() { ID = attribute.Attributes["AttrID"].Value };
                        // two specific groups of nodes may appear in attributes - name and description
                        // we don't want to save it as criterion
                        bool saveCriterion = true;

                        foreach (XmlNode attributePart in attribute)
                        {
                            var value = attributePart.Attributes["Value"].Value;

                            switch (attributePart.Name)
                            {
                                case "NAME":
                                    criterion.Name = value;
                                    break;
                                case "DESCRIPTION":
                                    criterion.Description = value;
                                    break;
                                case "CRITERION":
                                    criterion.CriterionDirection = value == "Cost" ? "c" : "g";
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
                            CriterionList.Add(criterion);
                        }
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    foreach (XmlNode instance in xmlNode)
                    {

                        Alternative alternative = new Alternative();
                        Dictionary<string, float> criteriaValuesDictionary = new Dictionary<string, float>();

                        foreach (XmlNode instancePart in instance)
                        {
                            var value = instancePart.Attributes["Value"].Value;
                            var attributeID = instancePart.Attributes["AttrID"].Value;
                            
                            if (attributeID == descriptionAttributeId)
                            {
                                alternative.Description = value;
                            } else if (attributeID == nameAttributeId)
                            {
                                alternative.Name = value;
                            } else
                            {
                                Criterion criterion = CriterionList.Find(element => element.ID == attributeID);
                                criteriaValuesDictionary.Add(criterion.Name, float.Parse(value, CultureInfo.InvariantCulture));
                            }
                        }

                        alternative.CriteriaValues = criteriaValuesDictionary;
                        AlternativeList.Add(alternative);
                    }
                }
            }
        }

        public void LoadUTX(string filePath)
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();

            //load XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@filePath);

            string descriptionAttributeName = "";

            // iterate on its nodes
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                if (xmlNode.Name == "ATTRIBUTES")
                {
                    foreach (XmlNode attribute in xmlNode)
                    {
                        Criterion criterion = new Criterion() { Name = attribute.Attributes["AttrID"].Value };
                        bool saveCriterion = true;

                        foreach (XmlNode attributePart in attribute)
                        {
                            var value = attributePart.Attributes["Value"].Value;

                            switch (attributePart.Name)
                            {
                                case "DESCRIPTION":
                                    criterion.Description = value;
                                    break;
                                case "CRITERION":
                                    criterion.CriterionDirection = value == "Cost" ? "c" : "g";
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
                                case "TYPE":
                                    break;
                                default:
                                    Console.WriteLine("Improper XML structure");
                                    return;
                            }
                        }

                        if (saveCriterion)
                        {
                            CriterionList.Add(criterion);
                        }
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    foreach (XmlNode instance in xmlNode)
                    {
                        Alternative alternative = new Alternative() {Name = instance.Attributes["ObjID"].Value};
                        Dictionary<string, float> criteriaValuesDictionary = new Dictionary<string, float>();

                        foreach (XmlNode instancePart in instance)
                        {
                            var value = instancePart.Attributes["Value"].Value;
                            var attributeName = instancePart.Attributes["AttrID"].Value;

                            if (attributeName == descriptionAttributeName)
                            {
                                alternative.Description = value;
                            }
                            else
                            {
                                Criterion criterion = CriterionList.Find(element => element.Name == attributeName);
                                criteriaValuesDictionary.Add(criterion.Name, float.Parse(value, CultureInfo.InvariantCulture));
                            }
                        }

                        alternative.CriteriaValues = criteriaValuesDictionary;
                        AlternativeList.Add(alternative);
                    }
                }
            }
        }

        public void setMinAndMaxCriterionValues()
        {
            for (int i = 0; i < CriterionList.Count; i++)
            {
                float min = 0, max = 0;
                string criterionName = CriterionList[i].Name;

                for (int j = 0; j < AlternativeList.Count; j++)
                {
                    float value = AlternativeList[j].CriteriaValues[criterionName];

                    if (value < min)
                    {
                        min = value;
                    }
                    if (value > max)
                    {
                        max = value;
                    }
                }
                CriterionList[i].MaxValue = max;
                CriterionList[i].MinValue = min;
            }
        }
    }
}
