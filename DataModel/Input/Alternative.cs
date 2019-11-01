﻿using System.Collections.Generic;

namespace DataModel.Input
{
    public class Alternative
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Alternative(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary> pairs: (criterion name, value) </summary>
        public Dictionary<string, float> CriteriaValues { get; set; }

    }
}
