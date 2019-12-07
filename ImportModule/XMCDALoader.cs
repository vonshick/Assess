using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class XMCDALoader
    {
        public List<Criterion> CriterionList { get; set; }
        public List<Alternative> AlternativeList { get; set; }
        public string XMCDADirectory { get; set; }

        public XMCDALoader(string xmcdaDirectory) 
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();
            XMCDADirectory = xmcdaDirectory;
        }

        public void setMinAndMaxCriterionValues()
        {
            for (int i = 0; i < CriterionList.Count; i++)
            {
                // 1 / 0 equals infinity
                // it is forbidden to divide by 0 as constant
                // but it is allowed to divide by variable equal to it
                float zero = 0, min = 1 / zero, max = - 1 / zero;

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

        private void LoadCriteria()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(XMCDADirectory, "criteria.xml"));

            // this file contains only one main block - <criteria>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                Criterion criterion = new Criterion()
                {
                    Name = xmlNode.Attributes["name"].Value,
                    ID = xmlNode.Attributes["id"].Value
                };
                
                CriterionList.Add(criterion);
            }
        }

        private void LoadCriteriaScales()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(XMCDADirectory, "criteria_scales.xml"));

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                string criterionID = xmlNode.ChildNodes[0].InnerText;
                string criterionDirection = xmlNode.ChildNodes[1].FirstChild.FirstChild.FirstChild.InnerText;

                var index = CriterionList.FindIndex(criterion => criterion.ID == criterionID);
                CriterionList[index].CriterionDirection = criterionDirection == "max" ? "g" : "c";
            }
        }

        private void LoadCriteriaThresholds()
        {

        }

        private void LoadAlternatives()
        {

        }

        private void LoadMethodParameteres()
        {

        }

        private void LoadPerformanceTable()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(XMCDADirectory, "performance_table.xml"));

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                Alternative alternative = new Alternative {CriteriaValues = new Dictionary<Criterion, float>()};

                foreach (XmlNode performance in xmlNode.ChildNodes)
                {
                    // first node containts alternative ID
                    if (performance.Name == "alternativeID")
                    {
                        alternative.Name = performance.InnerText;
                    }
                    else
                    {
                        string criterionID = performance.ChildNodes[0].InnerText;
                        Criterion matchingCriterion = CriterionList.Find(criterion => criterion.ID == criterionID);
                        float value = float.Parse(performance.ChildNodes[1].FirstChild.InnerText, CultureInfo.InvariantCulture);
                        alternative.CriteriaValues.Add(matchingCriterion, value);
                    }
                }
                
                AlternativeList.Add(alternative);
            }
        }

        private void LoadWeights()
        {

        }

        public void LoadXMCDA()
        {
            LoadCriteria();
            LoadCriteriaScales();
            LoadCriteriaThresholds();
            LoadAlternatives();
            LoadMethodParameteres();
            LoadPerformanceTable();
            LoadWeights();
        }
    }
}
