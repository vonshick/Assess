using System;
using System.Collections.Generic;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class CoefficientsDialog
    {
        public List<CriterionCoefficient> CriteriaCoefficientsList;
        public CoefficientsDisplayObject DisplayObject;
        private List<float> _bestValues;
        private List<float> _worstValues;
        private string _currentCriterionName;
        private float _lowerProbabilityBoundary;
        private float _upperProbabilityBoundary;
        private List<Criterion> _criterionList;

        public CoefficientsDialog(List<Criterion> criterionList)
        {
            _criterionList = criterionList;
            _bestValues = new List<float>();
            _worstValues = new List<float>();
            CriteriaCoefficientsList = new List<CriterionCoefficient>();

            for (int i = 0; i < _criterionList.Count; i++)
            {
                if (_criterionList[i].CriterionDirection == "Cost")
                {
                    _bestValues.Add(_criterionList[i].MinValue);
                    _worstValues.Add(_criterionList[i].MaxValue);
                }
                else
                {
                    _bestValues.Add(_criterionList[i].MaxValue);
                    _worstValues.Add(_criterionList[i].MinValue);
                }
            }
        }

        public void GetCoefficientsForCriteria()
        {
            for (int i = 0; i < _criterionList.Count; i++)
            {
                var valuesToCompare = _worstValues.ToArray();
                valuesToCompare[i] = _bestValues[i];

                DisplayObject = new CoefficientsDisplayObject(_criterionList, valuesToCompare, 0.5f);

                _currentCriterionName = _criterionList[i].Name;
                _lowerProbabilityBoundary = 0;
                _upperProbabilityBoundary = 1;

                ProcessDialog();
            }

            for (int i = 0; i < _criterionList.Count; i++)
                Console.WriteLine(CriteriaCoefficientsList[i].CriterionName + " : " + CriteriaCoefficientsList[i].Coefficient);
        }

        public string displayDialog()
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz WARIANT:");
            for (int i = 0; i < DisplayObject.CriterionNames.Length; i++)
            {
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.ValuesToCompare[i]);
            }

            Console.WriteLine("\nWpisz '2' jeśli wolisz LOTERIĘ");
            for (int i = 0; i < DisplayObject.CriterionNames.Length; i++)
            {
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.BestValues[i]);
            }

            Console.WriteLine("z prawdopodobienstwem " + DisplayObject.P + "\n");

            for (int i = 0; i < DisplayObject.CriterionNames.Length; i++)
            {
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.WorstValues[i]);
            }

            Console.WriteLine("z prawdopodobienstwem " + (1 - DisplayObject.P) + "\n");

            Console.WriteLine("'1', '2' lub 'n' :\n");

            return Console.ReadLine();
        }

        public void ProcessDialog()
        {
            string choice = displayDialog();

            if (choice.Equals("1"))
            {
                _lowerProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
                ProcessDialog();
            }
            else if (choice.Equals("2"))
            {
                _upperProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
                ProcessDialog();
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy wspolczynnik!\n");
                CriteriaCoefficientsList.Add(new CriterionCoefficient(_currentCriterionName, DisplayObject.P));
            }
        }

    }
}
