using System.Collections.Generic;

namespace DataModel.Input
{
    public class Alternative
    {
        public Alternative()
        {
        }

        public Alternative(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary> pairs: (criterion name, value) </summary>
        public Dictionary<Criterion, float> CriteriaValues { get; set; }

    }
}
