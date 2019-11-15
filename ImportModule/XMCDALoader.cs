using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    class XMCDALoader
    {
        public List<Criterion> CriterionList { get; set; }
        public List<Alternative> AlternativeList { get; set; }

        public string XMCDADirectory { get; set; }


        private void loadCriteria()
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

        private void loadCriteriaScales()
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

        private void loadCriteriaThresholds()
        {

        }

        private void loadAlternatives()
        {

        }

        private void loadMethodParameteres()
        {

        }

        private void loadPerformanceTable()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(XMCDADirectory, "performance_table.xml"));

            // this file contains only one main block - <criteriaScales>
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes[0])
            {
                Alternative alternative = new Alternative();
                alternative.CriteriaValues = new Dictionary<string, float>();

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
                        string criterionName = CriterionList.Find(criterion => criterion.ID == criterionID).Name;
                        float value = float.Parse(performance.ChildNodes[1].FirstChild.InnerText, CultureInfo.InvariantCulture);
                        alternative.CriteriaValues.Add(criterionName, value);
                    }
                }
                
                AlternativeList.Add(alternative);
            }
        }

        private void loadWeights()
        {

        }

        public void loadXMCDA(string xmcdaDirectory)
        {
            CriterionList = new List<Criterion>();
            AlternativeList = new List<Alternative>();
            XMCDADirectory = xmcdaDirectory;
            loadCriteria();
            loadCriteriaScales();
            loadCriteriaThresholds();
            loadAlternatives();
            loadMethodParameteres();
            loadPerformanceTable();
            loadWeights();
        }
    }
}
