using DataModel.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ImportModule
{
    public class CSVLoader : DataLoader
    {
        private int lineNumber;
        private char separator;
        private int numberOfColumns;

        public CSVLoader() : base()
        {
            lineNumber = 0;
        }

        private string[] ReadNewLine(StreamReader reader)
        {
            lineNumber++;
            string[] alternativeValues = reader.ReadLine().Split(separator);

            if (alternativeValues.Length != numberOfColumns)
            {
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException("Improper number of columns in line " + lineNumber.ToString() + " of CSV file.");
            }

            return (alternativeValues);
        }

        // validate if contain only 'c', 'g' and separator
        // if structure is correct set separator and expected number of columns
        private void validateStructure(string firstLine)
        {
            string removedCost = Regex.Replace(firstLine, "c", "");
            string removedCostAndGain = Regex.Replace(removedCost, "g", "");
            char[] separators = removedCostAndGain.ToCharArray();
            var separatorSet = new HashSet<char>(separators);

            if (separatorSet.Count != 1)
            {
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException("Improper criteria directions row - it should contain only 'c', 'g' and separator (e.g. ',', ';') characters.");
            }
            else
            {
                separator = separators[0];
                numberOfColumns = separators.Length + 1;
            }
        }

        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".csv");


            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {

                lineNumber++;
                string firstLine = reader.ReadLine();
                validateStructure(firstLine);

                string[] criterionDirectionsArray = firstLine.Split(separator);

                string[] criterionNamesArray = ReadNewLine(reader);
                // iterating from 1 because first column is empty
                for (int i = 1; i < criterionDirectionsArray.Length; i++)
                {
                    // for CSV ID and Name are the same value
                    criterionList.Add(new Criterion(checkCriteriaNamesUniqueness(criterionNamesArray[i]), criterionDirectionsArray[i]) { ID = checkCriteriaNamesUniqueness(criterionNamesArray[i]) });
                }

                while (!reader.EndOfStream)
                {
                    var values = ReadNewLine(reader);

                    Alternative alternative = new Alternative { Name = checkAlternativesNamesUniqueness(values[0]), CriteriaValuesList = new List<CriterionValue>() };
                    alternative.ID = alternative.Name;

                    for (int i = 0; i < criterionList.Count; i++)
                    {
                        checkIfValueIsValid(values[i + 1], criterionList[i].Name, alternative.Name);
                        alternative.CriteriaValuesList.Add(new CriterionValue(criterionList[i].Name, float.Parse(values[i + 1], CultureInfo.InvariantCulture)));
                    }

                    alternativeList.Add(alternative);
                }
            }
        }
    }
}