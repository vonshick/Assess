using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class CoefficientsDialog
    {
        private readonly List<float> _bestValues;
        private readonly List<Criterion> _criterionList;
        private string _currentCriterionName;
        private float _lowerProbabilityBoundary;
        private float _upperProbabilityBoundary;
        private readonly List<float> _worstValues;
        public List<CriterionCoefficient> CriteriaCoefficientsList;
        public DisplayObject DisplayObject;

        public CoefficientsDialog(List<Criterion> criterionList)
        {
            _criterionList = criterionList;
            _bestValues = new List<float>();
            _worstValues = new List<float>();
            CriteriaCoefficientsList = new List<CriterionCoefficient>();

            for (var i = 0; i < _criterionList.Count; i++)
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

        public CriterionCoefficient GetCoefficientsForCriterion(Criterion criterion)
        {
            return CriteriaCoefficientsList.First(crit => criterion.Name == crit.CriterionName);
        }

        public void SetInitialValues(Criterion criterion)
        {
            var index = _criterionList.IndexOf(criterion);
            var valuesToCompare = _worstValues.ToArray();
            valuesToCompare[index] = _bestValues[index];

            DisplayObject = new DisplayObject(_criterionList, valuesToCompare, 0.5f);

            _currentCriterionName = _criterionList[index].Name;
            _lowerProbabilityBoundary = 0;
            _upperProbabilityBoundary = 1;
        }

        //todo remove
        public string displayDialog()
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz WARIANT:");
            for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.ValuesToCompare[i]);

            Console.WriteLine("\nWpisz '2' jeśli wolisz LOTERIĘ");
            for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.BestValues[i]);

            Console.WriteLine("z prawdopodobienstwem " + DisplayObject.P + "\n");

            for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
                Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.WorstValues[i]);

            Console.WriteLine("z prawdopodobienstwem " + (1 - DisplayObject.P) + "\n");

            Console.WriteLine("'1', '2' lub 'n' :\n");

            return Console.ReadLine();
        }

        public void ProcessDialog(int choice)
        {
            if (choice == 1)
            {
                _lowerProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
            }
            else if (choice == 2)
            {
                _upperProbabilityBoundary = DisplayObject.P;
                DisplayObject.P = (_lowerProbabilityBoundary + _upperProbabilityBoundary) / 2;
            }
            else if (choice == 3)
            {
                Console.WriteLine("\nDodano nowy wspolczynnik: " + _currentCriterionName + " : " + DisplayObject.P);
                CriteriaCoefficientsList.Add(new CriterionCoefficient(_currentCriterionName, DisplayObject.P));
            }
            else
            {
                //TODO vonshick
                // remove the warning - it's useful only for developers
                throw new Exception("Assess: wrong choice ID passed to ProcessDialog()");
            }
        }
    }
}