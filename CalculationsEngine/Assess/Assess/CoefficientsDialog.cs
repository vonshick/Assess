using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class CoefficientsDialog : INotifyPropertyChanged
    {
        private readonly List<double> _bestValues;
        private readonly List<Criterion> _criterionList;
        private readonly List<double> _worstValues;
        private string _currentCriterionName;
        private DisplayObject _displayObject;
        private double _lowerProbabilityBoundary;
        private double _upperProbabilityBoundary;
        public List<CriterionCoefficient> CriteriaCoefficientsList;

        public CoefficientsDialog(List<Criterion> criterionList)
        {
            _criterionList = criterionList;
            _bestValues = new List<double>();
            _worstValues = new List<double>();
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


        public DisplayObject DisplayObject
        {
            get => _displayObject;
            set
            {
                _displayObject = value;
                OnPropertyChanged(nameof(DisplayObject));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
        //public string displayDialog()
        //{
        //    Console.WriteLine("Wpisz '1' jeśli wolisz WARIANT:");
        //    for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
        //        Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.ValuesToCompare[i]);

        //    Console.WriteLine("\nWpisz '2' jeśli wolisz LOTERIĘ");
        //    for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
        //        Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.BestValues[i]);

        //    Console.WriteLine("z prawdopodobienstwem " + DisplayObject.P + "\n");

        //    for (var i = 0; i < DisplayObject.CriterionNames.Length; i++)
        //        Console.WriteLine(DisplayObject.CriterionNames[i] + " = " + DisplayObject.WorstValues[i]);

        //    Console.WriteLine("z prawdopodobienstwem " + (1 - DisplayObject.P) + "\n");

        //    Console.WriteLine("'1', '2' lub 'n' :\n");

        //    return Console.ReadLine();
        //}

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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}