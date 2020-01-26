using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Input
{
    public class CriterionEnum
    {
        public string CriterionId { get; set; }
        public Dictionary<string, double> EnumDictionary { get; set; }

        public CriterionEnum(string criterionId)
        {
            CriterionId = criterionId;
            EnumDictionary = new Dictionary<string, double>();
        }
    }
}
