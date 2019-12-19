using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using DataModel.Input;

namespace ImportModule
{
    public class CSVLoader : DataLoader
    {
        private int lineNumber;
        private char separator;

        public CSVLoader() : base()
        {
            lineNumber = 0;
        }

        private string[] ReadNewLine(StreamReader reader)
        {
            lineNumber++;
            return (reader.ReadLine().Split(separator));
        }

        // In case when values are not separated by comma but by semicolon
        private void setSeparator(string firstLine)
        {
            if (firstLine.Contains(";") && !firstLine.Contains(","))
            {
                separator = ';';
            }
            else if (firstLine.Contains(",") && !firstLine.Contains(";"))
            {
                separator = ',';
            }
            else
            {
                Trace.WriteLine("File format is not valid! Values have to be separated by ';' or ','.");
                return;
            }
        }

        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {

                    lineNumber++;
                    string firstLine = reader.ReadLine();
                    setSeparator(firstLine);

                    string[] criterionDirectionsArray = firstLine.Split(separator);

                    string[] criterionNamesArray = ReadNewLine(reader);
                    // iterating from 1 because first column is empty
                    for (int i = 1; i < criterionDirectionsArray.Length; i++)
                    {
                        // for CSV ID and Name are the same value
                        criterionList.Add(new Criterion(criterionNamesArray[i], criterionDirectionsArray[i]) { ID = criterionNamesArray[i] });
                    }

                    while (!reader.EndOfStream)
                    {
                        var values = ReadNewLine(reader);

                        Alternative alternative = new Alternative { Name = values[0], CriteriaValues = new Dictionary<Criterion, float>() };

                        for (int i = 0; i < criterionList.Count; i++)
                        {
                            alternative.CriteriaValues.Add(criterionList[i], float.Parse(values[i + 1], CultureInfo.InvariantCulture));
                        }

                        alternativeList.Add(alternative);
                    }
                }
            }
            catch (Exception e)
            {
                //TODO vonshick WARNINGS
                // make warning more accurate 
                // if process failed while processing first line 
                // than it is possible that the structure of whole file is wrong
                if (lineNumber > 1)
                {
                    Trace.WriteLine("The process failed while processing line " + lineNumber.ToString() + " of CSV file");
                    Trace.WriteLine("Error: " + e.Message);
                }
                else
                {
                    Trace.WriteLine("Processing CSV file " + filePath + " failed.");
                    Trace.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}