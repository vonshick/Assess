using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DataModel.Input;

namespace ImportModule
{
    public class CSVLoader : DataLoader
    {
        private int lineNumber;
        private int numberOfColumns;
        private char separator;

        public CSVLoader()
        {
            lineNumber = 0;
        }

        private string[] ReadNewLine(StreamReader reader)
        {
            lineNumber++;
            var alternativeValues = reader.ReadLine().Split(separator);

            if (alternativeValues.Length != numberOfColumns)
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException("Improper number of columns in line " + lineNumber + " of CSV file.");

            return alternativeValues;
        }

        // validate if contain only 'c', 'g' and separator
        // if structure is correct set separator and expected number of columns
        private void validateStructure(string firstLine)
        {
            var removedCost = Regex.Replace(firstLine, "c", "");
            var removedCostAndGain = Regex.Replace(removedCost, "g", "");
            var separators = removedCostAndGain.ToCharArray();
            var separatorSet = new HashSet<char>(separators);

            if (separatorSet.Count != 1)
            {
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException(
                    "Improper criteria directions row - it should contain only 'c', 'g' and separator (e.g. ',', ';') characters.");
            }

            separator = separators[0];
            numberOfColumns = separators.Length + 1;
        }

        private string[] renameDirectionString(string[] directionStringArray)
        {
            var renamedDirectionStringList = new List<string>();
            foreach (var directionString in directionStringArray)
                if (directionString.Equals(""))
                {
                }
                else if (directionString.Equals("c"))
                {
                    renamedDirectionStringList.Add("Cost");
                }
                else if (directionString.Equals("g"))
                {
                    renamedDirectionStringList.Add("Gain");
                }
                else
                {
                    throw new ImproperFileStructureException(
                        "Improper criteria directions row - it should contain only 'c', 'g' and separator (e.g. ',', ';') characters.");
                }

            return renamedDirectionStringList.ToArray();
        }

        protected override void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".csv");


            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                lineNumber++;
                var firstLine = reader.ReadLine();
                validateStructure(firstLine);

                var criterionDirectionsArray = renameDirectionString(firstLine.Split(separator));

                var criterionNamesArray = ReadNewLine(reader);
                // iterating from 1 because first column is empty
                for (var i = 1; i < criterionDirectionsArray.Length; i++)
                    // for CSV ID and Name are the same value
                    criterionList.Add(new Criterion(checkCriteriaNamesUniqueness(criterionNamesArray[i]), criterionDirectionsArray[i])
                        {ID = checkCriteriaNamesUniqueness(criterionNamesArray[i]), LinearSegments = 1});

                while (!reader.EndOfStream)
                {
                    var values = ReadNewLine(reader);

                    var alternative = new Alternative
                    {
                        Name = checkAlternativesNamesUniqueness(values[0]), CriteriaValuesList = new ObservableCollection<CriterionValue>()
                    };
                    alternative.ID = alternative.Name;

                    for (var i = 0; i < criterionList.Count; i++)
                    {
                        checkIfValueIsValid(values[i + 1], criterionList[i].Name, alternative.Name);
                        alternative.CriteriaValuesList.Add(new CriterionValue(criterionList[i].Name,
                            float.Parse(values[i + 1], CultureInfo.InvariantCulture)));
                    }

                    alternativeList.Add(alternative);
                }
            }
        }
    }
}