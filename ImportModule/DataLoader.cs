using DataModel.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        protected void ValidateFilePath(string path)
        {
            if (!File.Exists(@path))
            {
                //TODO vonshick WARNINGS
                throw (new FileNotFoundException("File " + path + " does not exists!"));
            }
        }

        protected virtual void ProcessFile(string path)
        {

        }

        protected bool isNameUsed(string newName, string[] usedNames)
        {
            foreach (string usedName in usedNames)
            {
                if (newName.Equals(usedName))
                {
                    return true;
                }
            }

            return false;
        }

        protected string addSuffixToName(string newName, string[] usedNames)
        {
            bool nameAlreadyExists = isNameUsed(newName, usedNames);
            int counter = 1;
            string nameFreeToUse = newName;

            while (nameAlreadyExists)
            {
                nameFreeToUse = newName + "_" + counter.ToString();
                nameAlreadyExists = isNameUsed(nameFreeToUse, usedNames);
                counter++;
            }

            return nameFreeToUse;
        }

        protected string checkCriteriaIdsUniqueness(string id) 
        {
            string[] usedIds = criterionList.Select(criterion => criterion.ID).ToArray();
            foreach(string usedId in usedIds) 
            {
                if(id.Equals(usedId))
                {
                    throw new ImproperFileStructureException("Attribute ID '" + id + "' has been already used!");
                }
            }
            
            return id;
        }

        protected string checkCriteriaNamesUniqueness(string newName)
        {
            string[] usedNames = criterionList.Select(criterion => criterion.Name).ToArray();
            return (addSuffixToName(newName, usedNames));
        }
 
        protected string checkAlternativesNamesUniqueness(string newName)
        {
            string[] usedNames = alternativeList.Select(alternative => alternative.Name).ToArray();
            return (addSuffixToName(newName, usedNames));
        }


        public virtual void LoadData(string path)
        {
            ProcessFile(path);
            setMinAndMaxCriterionValues();
        }
    }
}
