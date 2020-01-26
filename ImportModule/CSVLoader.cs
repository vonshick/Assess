// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

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
                //TODO vonshick WARNINGS
                throw new ImproperFileStructureException(
                    "Improper criteria directions row - it should contain only 'c', 'g' and separator (e.g. ',', ';') characters.");

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
                        {ID = checkCriteriaNamesUniqueness(criterionNamesArray[i])});

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
                            double.Parse(values[i + 1], CultureInfo.InvariantCulture)));
                    }

                    alternativeList.Add(alternative);
                }
            }
        }
    }
}