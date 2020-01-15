using System;
using System.Collections.Generic;
using DataModel.Input;
using DataModel.Results;

namespace CalculationsEngine.Assess.Assess
{
    public class DialogController
    {
        private readonly int _methodId;
        private readonly PartialUtilityValues _oneUtilityPoint;
        private double _p;
        private readonly PartialUtilityValues _zeroUtilityPoint;
        public ConstantProbabilityDialog ConstantProbabilityDialog;
        public DisplayObject DisplayObject;
        public LotteriesComparisonDialog LotteriesComparisonDialog;
        public List<PartialUtilityValues> PointsList;
        public ProbabilityComparisonDialog ProbabilityComparisonDialog;
        public VariableProbabilityDialog VariableProbabilityDialog;


        // methodId - integer from 1 - 4
        // 1 - constant probability 
        // 2 - variable probability
        // 3 - lotteries comparison
        // 4 - probability comparison
        public DialogController(Criterion criterion, int methodId, double p = 0)
        {
            if (criterion.CriterionDirection.Equals("Cost"))
            {
                _zeroUtilityPoint = new PartialUtilityValues(criterion.MaxValue, 0);
                _oneUtilityPoint = new PartialUtilityValues(criterion.MinValue, 1);
            }
            else
            {
                _zeroUtilityPoint = new PartialUtilityValues(criterion.MaxValue, 1);
                _oneUtilityPoint = new PartialUtilityValues(criterion.MinValue, 0);
            }

            _methodId = methodId;
            DisplayObject = new DisplayObject();
            PointsList = new List<PartialUtilityValues>();
            DisplayObject.PointsList = PointsList;
            PointsList.Add(_zeroUtilityPoint);
            PointsList.Add(_oneUtilityPoint);
            DisplayObject.P = p;

            switch (_methodId)
            {
                case 1:
                    createConstantProbabilityObject();
                    break;
                case 2:
                    createVariableProbabilityObject();
                    break;
                case 3:
                    createLotteriesComparisonObject();
                    break;
                case 4:
                    createProbabilityComparisonObject();
                    break;
                default:
                    throw new Exception("Assess error: wrong dialog method chosen.");
            }
        }

        // firstPoint and secondPoint are edges of chosen utility function segment
        public Dialog TriggerDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            switch (_methodId)
            {
                case 1:
                    return triggerConstantProbabilityDialog(firstPoint, secondPoint);
                case 2:
                    return triggerVariableProbabilityDialog(firstPoint, secondPoint);
                case 3:
                    return triggerLotteriesComparisonDialog(firstPoint, secondPoint);
                case 4:
                    return triggerProbabilityComparisonDialog(firstPoint, secondPoint);
                default:
                    throw new Exception("Assess error: wrong dialog method chosen.");
            }
        }


        private void setLotteriesComparisonInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            var edgeValuesLottery = new Lottery(_zeroUtilityPoint, _oneUtilityPoint);
            edgeValuesLottery.SetProbability((upperUtilityPoint.Y + lowerUtilityPoint.Y) / 2 * DisplayObject.P);

            var mediumUtilityPoint = new PartialUtilityValues((upperUtilityPoint.X + lowerUtilityPoint.X) / 2, -1);
            var comparisonLottery = new Lottery(_zeroUtilityPoint, mediumUtilityPoint);
            comparisonLottery.SetProbability(DisplayObject.P);

            DisplayObject.EdgeValuesLottery = edgeValuesLottery;
            DisplayObject.ComparisonLottery = comparisonLottery;
        }

        private void createLotteriesComparisonObject()
        {
            setLotteriesComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
            LotteriesComparisonDialog = new LotteriesComparisonDialog(_zeroUtilityPoint.Y, DisplayObject.P, DisplayObject);
        }

        public Dialog triggerLotteriesComparisonDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setLotteriesComparisonInput(firstPoint, secondPoint);
            return LotteriesComparisonDialog;
//            LotteriesComparisonDialog.ProcessDialog();
        }


        private void setProbabilityComparisonInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.Y + upperUtilityPoint.Y) / 2);
        }

        private void createProbabilityComparisonObject()
        {
            setProbabilityComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
            ProbabilityComparisonDialog = new ProbabilityComparisonDialog(_zeroUtilityPoint.Y, _oneUtilityPoint.Y, DisplayObject);
        }

        public Dialog triggerProbabilityComparisonDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setProbabilityComparisonInput(firstPoint, secondPoint);
            return ProbabilityComparisonDialog;
//            ProbabilityComparisonDialog.ProcessDialog();
        }


        private void setConstantProbabilityInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability(DisplayObject.P);
        }

        private void createConstantProbabilityObject()
        {
            setConstantProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
            ConstantProbabilityDialog = new ConstantProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
        }

        public Dialog triggerConstantProbabilityDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setConstantProbabilityInput(firstPoint, secondPoint);
            ConstantProbabilityDialog.SetInitialValues();
            return ConstantProbabilityDialog;
//            ConstantProbabilityDialog.ProcessDialog();
        }


        private void setVariableProbabilityInput(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            var upperUtilityPoint = firstPoint.Y > secondPoint.Y ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.Y < secondPoint.Y ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.Y + upperUtilityPoint.Y) / 2);
        }

        private void createVariableProbabilityObject()
        {
            setVariableProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
            VariableProbabilityDialog = new VariableProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
        }

        public Dialog triggerVariableProbabilityDialog(PartialUtilityValues firstPoint, PartialUtilityValues secondPoint)
        {
            setVariableProbabilityInput(firstPoint, secondPoint);
            return VariableProbabilityDialog;
//            VariableProbabilityDialog.ProcessDialog();
        }
    }
}