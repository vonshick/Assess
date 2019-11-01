using System.Collections.Generic;
using UTA.Models;

namespace UTA.ViewModels
{
    public class VariantsTestList
    {
        public List<VariantTest> VList { get; set; }

        public VariantsTestList()
        {
            VList = new List<VariantTest>
            {
                new VariantTest("variant 1"),
                new VariantTest("variant 2")
            };
        }
    }
}
