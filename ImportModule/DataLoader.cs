using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class DataLoader
    {
        public List<Criterion> CriterionList { get; set; }
        public List<Alternative> AlternativeList { get; set; }
        private int lineNumber;
        private char separator;

        public DataLoader()
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();
            lineNumber = 0;
        }

        private string[] ReadNewLine(StreamReader reader) {
            lineNumber++;
            return(reader.ReadLine().Split(separator));
        }

        // In case when values are not separated by comma but by semicolon
        private void setSeparator(string firstLine)
        {
            if(firstLine.Contains(";") && !firstLine.Contains(",")) 
            {
                separator = ';';
            }
            else if(firstLine.Contains(",") && !firstLine.Contains(";")) 
            {
                separator = ',';
            } else {
                Trace.WriteLine("File format is not valid! Values have to be separated by ';' or ','.");
                return;
            }
        }

        public void LoadCSV(string filePath)
        {
            try {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {

                    lineNumber++;
                    string firstLine = reader.ReadLine();
                    setSeparator(firstLine);

                    string[] criterionDirectionsArray = firstLine.Split(separator);

                    string[] criterionNamesArray = ReadNewLine(reader);
                    // iterating from 1 because first column is empty
                    for (int i = 1; i < criterionDirectionsArray.Length; i++)
                    {
                        CriterionList.Add(new Criterion(criterionNamesArray[i], criterionDirectionsArray[i]));
                    }

                    while (!reader.EndOfStream)
                    {
                        var values = ReadNewLine(reader);

                        Alternative alternative = new Alternative {Name = values[0], CriteriaValues = new Dictionary<Criterion, float>()};

                        for (int i = 0; i < CriterionList.Count; i++)
                        {
                            alternative.CriteriaValues.Add(CriterionList[i], float.Parse(values[i + 1], CultureInfo.InvariantCulture));
                        }

                        AlternativeList.Add(alternative);
                    }
                }
            } catch (Exception e) {
                Trace.WriteLine("The process failed while processing line " + lineNumber.ToString() + " of CSV file");
                Trace.WriteLine("Error: " + e.ToString());
            }
        }

        public void LoadXML(string filePath)
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
                                    if(value == "Cost" || value == "Gain") {
                                    criterion.CriterionDirection = value == "Cost" ? "c" : "g";
                                    } else {
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
                            CriterionList.Add(criterion);
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
                            } else if (attributeID == nameAttributeId)
                            {
                                alternative.Name = value;
                            } else
                            {
                                Criterion criterion = CriterionList.Find(element => element.ID == attributeID);
                                criteriaValuesDictionary.Add(criterion, float.Parse(value, CultureInfo.InvariantCulture));
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
                        Dictionary<string, string> enumIdsNamesDictionary = new Dictionary<string, string>();
                        Dictionary<string, float> enumIdsValuesDictionary = new Dictionary<string, float>();
                        
                        foreach (XmlNode attributePart in attribute)
                        {
                            var value = attributePart.Attributes["Value"].Value;

                            switch (attributePart.Name)
                            {
                                case "TYPE":
                                    if(value == "Enum") {
                                        criterion.IsEnum = true;
                                        foreach (XmlNode enumName in attributePart) {
                                            enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value, enumName.Attributes["Value"].Value);
                                        }
                                    }
                                    break;
                                case "DESCRIPTION":
                                    criterion.Description = value;
                                    break;
                                case "CRITERION":
                                    // if value is enum type
                                    if(value == "Rank"){
                                        foreach (XmlNode enumValue in attributePart) {
                                            enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value, float.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                        }
                                        criterion.CriterionDirection = "c";
                                    } else {
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
                                default:
                                    Console.WriteLine("Improper XML structure");
                                    return;
                            }
                        }

                        if (saveCriterion)
                        {
                            if(criterion.IsEnum) {
                                criterion.EnumDictionary = new Dictionary<string, float>();
                                foreach(KeyValuePair<string, string> entry in enumIdsNamesDictionary)
                                {
                                    string enumID = entry.Key;
                                    string enumName = entry.Value;
                                    float enumValue = enumIdsValuesDictionary[enumID];
                                    criterion.EnumDictionary.Add(enumName, enumValue);
                                }   
                            } 
                            CriterionList.Add(criterion);
                        }
                    }
                }
                else if (xmlNode.Name == "OBJECTS")
                {
                    foreach (XmlNode instance in xmlNode)
                    {
                        Alternative alternative = new Alternative() {Name = instance.Attributes["ObjID"].Value};
                        Dictionary<Criterion, float> criteriaValuesDictionary = new Dictionary<Criterion, float>();

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
                                if(criterion.IsEnum) {
                                    float enumValue = criterion.EnumDictionary[value];
                                    //so far we save only numerical value of enum in attribute
                                    criteriaValuesDictionary.Add(criterion, enumValue);
                                } else {
                                    criteriaValuesDictionary.Add(criterion, float.Parse(value, CultureInfo.InvariantCulture));
                                }
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
                // 1 / 0 equals infinity
                // it is forbidden to divide by 0 as constant
                // but it is allowed to divide by variable equal to it
                float min = float.PositiveInfinity, max = float.NegativeInfinity;

                for (int j = 0; j < AlternativeList.Count; j++)
                {
                    float value = AlternativeList[j].CriteriaValues[CriterionList[i]];

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
