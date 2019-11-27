using System.Collections.Generic;

namespace DataModel.Input
{
    public class AlternativeEntry
    {
        public AlternativeEntry()
        {
        }

        public AlternativeEntry(string name, List<AlternativeValue> listOfAlternativeValue)
        {
            Name = name;
            ListAlternativeValue = listOfAlternativeValue;
        }

        public string Name { get; set; }
        public List<AlternativeValue> ListAlternativeValue { get; set; }
    }
}