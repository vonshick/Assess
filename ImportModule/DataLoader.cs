using System.Collections.Generic;
using DataModel.Input;

namespace ImportModule
{
    public class DataLoader
    {
        protected List<Criterion> criterionList;
        protected List<Alternative> alternativeList;

        public List<Criterion> CriterionList
        {
            get { return criterionList; }
        }

        public List<Alternative> AlternativeList
        {
            get { return alternativeList; }
        }

        public DataLoader()
        {
            criterionList = new List<Criterion>();
            alternativeList = new List<Alternative>();
        }

        protected void setMinAndMaxCriterionValues()
        {
            for (int i = 0; i < criterionList.Count; i++)
            {
                float min = float.PositiveInfinity, max = float.NegativeInfinity;

                for (int j = 0; j < alternativeList.Count; j++)
                {
                    float value = alternativeList[j].CriteriaValues[criterionList[i]];

                    if (value < min)
                    {
                        min = value;
                    }
                    if (value > max)
                    {
                        max = value;
                    }
                }
                
                criterionList[i].MaxValue = max;
                criterionList[i].MinValue = min;
            }
        }

        protected virtual void ProcessFile(string path) 
        {

        }
        
        public virtual void LoadData(string path)
        {
            ProcessFile(path);
            setMinAndMaxCriterionValues();
        }
    }
}
