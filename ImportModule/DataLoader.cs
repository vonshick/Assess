using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataModel.Input;
using DataModel.Results;

namespace ImportModule
{
    public class DataLoader
    {
        protected List<Alternative> alternativeList;
        protected List<Criterion> criterionList;
        protected Results results;

        public DataLoader()
        {
            criterionList = new List<Criterion>();
            alternativeList = new List<Alternative>();
            results = new Results();
        }

        public List<Criterion> CriterionList => criterionList;

        public List<Alternative> AlternativeList => alternativeList;

        public Results Results => results;

        protected void setMinAndMaxCriterionValues()
        {
            for (var i = 0; i < criterionList.Count; i++)
            {
                double min = double.PositiveInfinity, max = double.NegativeInfinity;

                for (var j = 0; j < alternativeList.Count; j++)
                {
                    var matchingCriterionValue = alternativeList[j].CriteriaValuesList
                        .First(criterionValue => criterionValue.Name == criterionList[i].Name);
                    if (matchingCriterionValue != null)
                    {
                        var value = (double) matchingCriterionValue.Value;

                        if (value < min) min = value;
                        if (value > max) max = value;
                    }
                    else
                    {
                        throw new Exception("There was no value for criterion " + criterionList[i].Name + " in alternative " +
                                            alternativeList[j].Name + ".");
                    }
                }

                criterionList[i].MaxValue = max;
                criterionList[i].MinValue = min;
            }
        }

        protected void ValidateFilePath(string path)
        {
            if (!File.Exists(path))
                //TODO vonshick WARNINGS
                throw new FileNotFoundException("File " + Path.GetFileName(path) + " does not exist.");
        }

        // expectedExtension is a param that contains string in format like ".csv", ".utx", ".xml"
        protected void ValidateFileExtension(string path, string expectedExtension)
        {
            if (!Path.GetExtension(path).Equals(expectedExtension))
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException("Wrong extension of the file " + Path.GetFileName(path) +
                                                         ". Expected extension: " + expectedExtension + ".");
        }

        protected virtual void ProcessFile(string path)
        {
        }

        protected bool isNameUsed(string newName, string[] usedNames)
        {
            foreach (var usedName in usedNames)
                if (newName.Equals(usedName))
                    return true;

            return false;
        }

        protected string addSuffixToName(string newName, string[] usedNames)
        {
            var nameAlreadyExists = isNameUsed(newName, usedNames);
            var counter = 1;
            var nameFreeToUse = newName;

            while (nameAlreadyExists)
            {
                nameFreeToUse = newName + "_" + counter;
                nameAlreadyExists = isNameUsed(nameFreeToUse, usedNames);
                counter++;
            }

            return nameFreeToUse;
        }

        protected string checkCriteriaIdsUniqueness(string id)
        {
            if (id.Equals("")) throw new ImproperFileStructureException("Criterion ID can not be an empty string.");

            var usedIds = criterionList.Select(criterion => criterion.ID).ToArray();
            foreach (var usedId in usedIds)
                if (id.Equals(usedId))
                    throw new ImproperFileStructureException("Criterion ID '" + id + "' has been already used.");

            return id;
        }


        protected string checkAlternativesIdsUniqueness(string id)
        {
            if (id.Equals("")) throw new ImproperFileStructureException("Alternative ID can not be an empty string.");

            var usedIds = alternativeList.Select(alternative => alternative.ID).ToArray();
            foreach (var usedId in usedIds)
                if (id.Equals(usedId))
                    throw new ImproperFileStructureException("Alternative ID '" + id + "' has been already used.");

            return id;
        }

        protected void checkIfValueIsValid(string value, string criterionId, string alternativeId)
        {
            if (value.Equals(""))
                throw new ImproperFileStructureException("Value can not be empty. Alternative " + alternativeId + ", criterion " +
                                                         criterionId + ".");

            double output = 0;
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
                throw new ImproperFileStructureException("Improper value format '" + value +
                                                         "'. Value has to be floating point. Alternative " + alternativeId +
                                                         ", criterion " + criterionId + ".");
        }

        protected string checkCriteriaNamesUniqueness(string newName)
        {
            if (newName.Equals("")) throw new ImproperFileStructureException("Criterion name can not be an empty string.");

            var usedNames = criterionList.Select(criterion => criterion.Name).ToArray();
            return addSuffixToName(newName, usedNames);
        }

        protected string checkAlternativesNamesUniqueness(string newName)
        {
            if (newName.Equals("")) throw new ImproperFileStructureException("Alternative name can not be an empty string.");

            var usedNames = alternativeList.Select(alternative => alternative.Name).ToArray();
            return addSuffixToName(newName, usedNames);
        }

        public virtual void LoadData(string path)
        {
            ProcessFile(path);
            setMinAndMaxCriterionValues();
        }
    }
}