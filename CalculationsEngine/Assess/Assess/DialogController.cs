using System.Collections.Generic;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class DialogController
    {
        public DisplayObject DisplayObject;
        public List<Point> PointsList;
        private Point _zeroUtilityPoint;
        private Point _oneUtilityPoint;
        private float _p;
        private int _methodId;
        public LotteriesComparisonDialog LotteriesComparisonDialog;
        public ProbabilityComparisonDialog ProbabilityComparisonDialog;
        public ConstantProbabilityDialog ConstantProbabilityDialog;
        public VariableProbabilityDialog VariableProbabilityDialog;


        // methodId - integer from 1 - 4
        // 1 - constant probability 
        // 2 - variable probability
        // 3 - lotteries comparison
        // 4 - probability comparison
        public DialogController(Criterion criterion, int methodId, float p = 0)
        {
            if (criterion.CriterionDirection.Equals("Cost"))
            {
                _zeroUtilityPoint = new Point(criterion.MaxValue, 0);
                _oneUtilityPoint = new Point(criterion.MinValue, 1);
            }
            else
            {
                _zeroUtilityPoint = new Point(criterion.MaxValue, 1);
                _oneUtilityPoint = new Point(criterion.MinValue, 0);
            }

            _methodId = methodId;
            DisplayObject = new DisplayObject();
            PointsList = new List<Point>();
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
                    throw new System.Exception("Assess error: wrong dialog method chosen.");
            }
        }

        // firstPoint and secondPoint are edges of chosen utility function segment
        public Dialog TriggerDialog(Point firstPoint, Point secondPoint)
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
                    throw new System.Exception("Assess error: wrong dialog method chosen.");
            }
        }


        private void setLotteriesComparisonInput(Point firstPoint, Point secondPoint)
        {
            var upperUtilityPoint = firstPoint.U > secondPoint.U ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.U < secondPoint.U ? firstPoint : secondPoint;

            var edgeValuesLottery = new Lottery(_zeroUtilityPoint, _oneUtilityPoint);
            edgeValuesLottery.SetProbability((float)((upperUtilityPoint.U + lowerUtilityPoint.U) / 2 * DisplayObject.P));

            var mediumUtilityPoint = new Point((upperUtilityPoint.X + lowerUtilityPoint.X) / 2, -1);
            var comparisonLottery = new Lottery(_zeroUtilityPoint, mediumUtilityPoint);
            comparisonLottery.SetProbability(DisplayObject.P);

            DisplayObject.EdgeValuesLottery = edgeValuesLottery;
            DisplayObject.ComparisonLottery = comparisonLottery;
        }

        private void createLotteriesComparisonObject()
        {
            setLotteriesComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
            LotteriesComparisonDialog = new LotteriesComparisonDialog(_zeroUtilityPoint.U, DisplayObject.P, DisplayObject);
        }

        public Dialog triggerLotteriesComparisonDialog(Point firstPoint, Point secondPoint)
        {
            setLotteriesComparisonInput(firstPoint, secondPoint);
            return LotteriesComparisonDialog;
//            LotteriesComparisonDialog.ProcessDialog();
        }



        private void setProbabilityComparisonInput(Point firstPoint, Point secondPoint)
        {
            var upperUtilityPoint = firstPoint.U > secondPoint.U ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.U < secondPoint.U ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.U + upperUtilityPoint.U) / 2);
        }

        private void createProbabilityComparisonObject()
        {
            setProbabilityComparisonInput(_zeroUtilityPoint, _oneUtilityPoint);
            ProbabilityComparisonDialog = new ProbabilityComparisonDialog(_zeroUtilityPoint.U, _oneUtilityPoint.U, DisplayObject);
        }

        public Dialog triggerProbabilityComparisonDialog(Point firstPoint, Point secondPoint)
        {
            setProbabilityComparisonInput(firstPoint, secondPoint);
            return ProbabilityComparisonDialog;
//            ProbabilityComparisonDialog.ProcessDialog();
        }



        private void setConstantProbabilityInput(Point firstPoint, Point secondPoint)
        {
            var upperUtilityPoint = firstPoint.U > secondPoint.U ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.U < secondPoint.U ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability(DisplayObject.P);
        }

        private void createConstantProbabilityObject()
        {
            setConstantProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
            ConstantProbabilityDialog = new ConstantProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
        }

        public Dialog triggerConstantProbabilityDialog(Point firstPoint, Point secondPoint)
        {
            setConstantProbabilityInput(firstPoint, secondPoint);
            ConstantProbabilityDialog.SetInitialValues();
            return ConstantProbabilityDialog;
//            ConstantProbabilityDialog.ProcessDialog();
        }



        private void setVariableProbabilityInput(Point firstPoint, Point secondPoint)
        {
            var upperUtilityPoint = firstPoint.U > secondPoint.U ? firstPoint : secondPoint;
            var lowerUtilityPoint = firstPoint.U < secondPoint.U ? firstPoint : secondPoint;

            DisplayObject.Lottery = new Lottery(lowerUtilityPoint, upperUtilityPoint);
            DisplayObject.Lottery.SetProbability((lowerUtilityPoint.U + upperUtilityPoint.U) / 2);
        }

        private void createVariableProbabilityObject()
        {
            setVariableProbabilityInput(_zeroUtilityPoint, _oneUtilityPoint);
            VariableProbabilityDialog = new VariableProbabilityDialog(_zeroUtilityPoint.X, _oneUtilityPoint.X, DisplayObject);
        }

        public Dialog triggerVariableProbabilityDialog(Point firstPoint, Point secondPoint)
        {
            setVariableProbabilityInput(firstPoint, secondPoint);
            return VariableProbabilityDialog;
//            VariableProbabilityDialog.ProcessDialog();
        }
    }
}