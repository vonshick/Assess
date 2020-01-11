using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    public class AssessDialog
    {
        private int chosenMethod;
        private float constantP;
        public List<Point> PointsList;
        public Point ZeroUtilityPoint;
        public Point OneUtilityPoint;
        public string CriterionName;
        public UtilityFunction UtilityFunction;
        public bool ContinueDialog;
        public List<CriterionCoefficient> CriteriaCoefficientsList;

        public AssessDialog()
        {

        }

        public AssessDialog(Criterion criterion)
        {
            ContinueDialog = true;

            if (criterion.CriterionDirection.Equals("Cost"))
            {
                ZeroUtilityPoint = new Point(criterion.MaxValue, 0);
                OneUtilityPoint = new Point(criterion.MinValue, 1);
            }
            else
            {
                ZeroUtilityPoint = new Point(criterion.MaxValue, 1);
                OneUtilityPoint = new Point(criterion.MinValue, 0);
            }

            CriterionName = criterion.Name;
            PointsList = new List<Point>();
            PointsList.Add(ZeroUtilityPoint);
            PointsList.Add(OneUtilityPoint);
        }

        private int ChoiceMethodDialog()
        {
            Console.WriteLine("");
            Console.WriteLine("Which method would you like to use?");
            Console.WriteLine("1. Variable probability");
            Console.WriteLine("2. Constant probability");
            Console.WriteLine("3. Lotteries comparison");
            Console.WriteLine("4. Probability comparison");
            Console.WriteLine("");
            Console.WriteLine("Pass '1', '2', '3' or '4'");
            Console.WriteLine("");

            int output;
            if (!Int32.TryParse(Console.ReadLine(), out output))
            {
                Console.WriteLine("Wrong input passed!");
                Console.WriteLine("");
                ChoiceMethodDialog();
            };

            Console.WriteLine("");

            return output;
        }

        public Lottery CreateLottery(int pointIndex)
        {
            Point lowerUtilityValue, upperUtilityValue;

            if (PointsList[pointIndex].U > PointsList[pointIndex + 1].U)
            {
                lowerUtilityValue = PointsList[pointIndex + 1];
                upperUtilityValue = PointsList[pointIndex];
            }
            else
            {
                lowerUtilityValue = PointsList[pointIndex];
                upperUtilityValue = PointsList[pointIndex + 1];
            }

            return new Lottery(lowerUtilityValue, upperUtilityValue);
        }

        public void DialogForTwoPoints()
        {
            if (PointsList.Count == 2)
            {
                chosenMethod = ChoiceMethodDialog();
                if (chosenMethod == 2 || chosenMethod == 3)
                {
                    Console.WriteLine("Pass p value (floating point from 0 to 1)");
                    constantP = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
                    Console.WriteLine("");
                }
            }

            Console.WriteLine("Który punkt jest początkiem interesującego Cię odcinka?");
            for (int i = 0; i < (PointsList.Count - 1); i++)
            {
                Console.WriteLine(i + ". (" + PointsList[i].X + ", " + PointsList[i].U + ")");
            }

            Console.WriteLine("\nPodaj odpowiedni indeks lub 'e' jeśli chcesz zakonczyc tworzenie funkcji.");

            string choice = Console.ReadLine();

            if (choice.Equals("e"))
            {
                Console.WriteLine("\nZakonczono tworzenie funkcji\n");
                UtilityFunction = new UtilityFunction(CriterionName, PointsList);
                ContinueDialog = false;
                return;
            }

            int pointIndex = Int32.Parse(choice);
            Console.WriteLine("");

            if (pointIndex >= (PointsList.Count - 1) || pointIndex < 0)
            {
                Console.WriteLine("Podano niewłaściwy indeks.");
                DialogForTwoPoints();
            }

            Lottery lottery = CreateLottery(pointIndex);

            switch (chosenMethod)
            {
                case 1:
                    lottery.SetProbability((lottery.LowerUtilityValue.U + lottery.UpperUtilityValue.U) / 2);
                    TalkToMeVariableProbability(lottery, lottery.LowerUtilityValue.X, lottery.UpperUtilityValue.X);
                    break;
                case 2:
                    lottery.SetProbability(constantP);
                    TalkToMeConstantProbability(lottery, lottery.LowerUtilityValue.X, lottery.UpperUtilityValue.X);
                    break;
                case 3:
                    Lottery edgeValuesLottery = new Lottery(ZeroUtilityPoint, OneUtilityPoint);
                    edgeValuesLottery.SetProbability((float)((lottery.UpperUtilityValue.U + lottery.LowerUtilityValue.U) / 2 * constantP));

                    Point mediumUtilityPoint = new Point((lottery.UpperUtilityValue.X + lottery.LowerUtilityValue.X) / 2, -1);
                    Lottery comparisonLottery = new Lottery(ZeroUtilityPoint, mediumUtilityPoint);
                    comparisonLottery.SetProbability(constantP);

                    TalkToMeLotteriesComparison(edgeValuesLottery, comparisonLottery, lottery.LowerUtilityValue.U, constantP);
                    break;
                case 4:
                    lottery.SetProbability((lottery.LowerUtilityValue.U + lottery.UpperUtilityValue.U) / 2);
                    TalkToMeProbabilityComparison(lottery, lottery.LowerUtilityValue.U, lottery.UpperUtilityValue.U);
                    break;
                default:
                    Console.WriteLine("Wrong value passed!");
                    Console.WriteLine("");
                    chosenMethod = ChoiceMethodDialog();
                    DialogForTwoPoints();
                    break;
            }

        }

        private string displayChoices(Lottery lottery, float x)
        {
            Console.WriteLine("Wpisz 'l' jeśli wolisz LOTERIĘ:");
            Console.WriteLine(lottery.UpperUtilityValue.X + " z prawdopodobienstwem " + lottery.P);
            Console.WriteLine(lottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - lottery.P) + "\n");
            Console.WriteLine("Wpisz 'r' jeśli wolisz równoważnik pewności:");
            Console.WriteLine(x + "\n");
            Console.WriteLine("Wpisz 'n' jeśli loteria i równoważnik pewności są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'l', 'n' lub 'r' :\n");

            string choice = Console.ReadLine();

            return choice;
        }

        public void TalkToMeVariableProbability(Lottery lottery, float lowerUtilityBoundary, float upperUtilityBoundary)
        {
            float x = (lowerUtilityBoundary + upperUtilityBoundary) / 2;
            string choice = displayChoices(lottery, x);

            if (choice.Equals("r"))
            {
                TalkToMeVariableProbability(lottery, lowerUtilityBoundary, x);
            }
            else if (choice.Equals("l"))
            {
                TalkToMeVariableProbability(lottery, x, upperUtilityBoundary);
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy punkt!\n");
                PointsList.Add(new Point(x, lottery.NewPointUtility()));
                PointsList.Sort((first, second) => first.X.CompareTo(second.X));
            }
            else
            {
                Console.WriteLine("\nPodano bledna wartosc!\n");
                TalkToMeVariableProbability(lottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
        }

        public void TalkToMeConstantProbability(Lottery lottery, float lowerUtilityBoundary, float upperUtilityBoundary)
        {
            float x = lowerUtilityBoundary + (upperUtilityBoundary - lowerUtilityBoundary) * lottery.P;
            string choice = displayChoices(lottery, x);

            if (choice.Equals("r"))
            {
                TalkToMeConstantProbability(lottery, lowerUtilityBoundary, x);
            }
            else if (choice.Equals("l"))
            {
                TalkToMeConstantProbability(lottery, x, upperUtilityBoundary);
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy punkt!\n");
                PointsList.Add(new Point(x, lottery.NewPointUtility()));
                PointsList.Sort((first, second) => first.X.CompareTo(second.X));
            }
            else
            {
                Console.WriteLine("\nPodano bledna wartosc!\n");
                TalkToMeConstantProbability(lottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
        }

        public string displayChoicesProbabilityComparison(Lottery lottery, float x)
        {
            Console.WriteLine("Wpisz 'l' jeśli wolisz LOTERIĘ:");
            Console.WriteLine(lottery.UpperUtilityValue.X + " z prawdopodobienstwem " + lottery.P);
            Console.WriteLine(lottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - lottery.P) + "\n");
            Console.WriteLine("Wpisz 'r' jeśli wolisz równoważnik pewności:");
            Console.WriteLine(x + "\n");
            Console.WriteLine("Wpisz 'n' jeśli loteria i równoważnik pewności są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'l', 'n' lub 'r' :\n");

            return Console.ReadLine();
        }

        public void TalkToMeProbabilityComparison(Lottery lottery, float lowerUtilityBoundary, float upperUtilityBoundary)
        {
            float x = (lottery.LowerUtilityValue.X + lottery.UpperUtilityValue.X) / 2;
            string choice = displayChoicesProbabilityComparison(lottery, x);

            if (choice.Equals("r"))
            {
                lowerUtilityBoundary = lottery.P;
                lottery.P = (lowerUtilityBoundary + upperUtilityBoundary) / 2;
                TalkToMeProbabilityComparison(lottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
            else if (choice.Equals("l"))
            {
                upperUtilityBoundary = lottery.P;
                lottery.P = (lowerUtilityBoundary + upperUtilityBoundary) / 2;
                TalkToMeProbabilityComparison(lottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy punkt!\n");
                PointsList.Add(new Point(x, lottery.NewPointUtility()));
                PointsList.Sort((first, second) => first.X.CompareTo(second.X));
            }
            else
            {
                Console.WriteLine("\nPodano bledna wartosc!\n");
                TalkToMeProbabilityComparison(lottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
        }

        public string displayChoicesLotteriesComparison(Lottery edgeValuesLottery, Lottery comparisonLottery)
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz loterię:");
            Console.WriteLine(comparisonLottery.UpperUtilityValue.X + " z prawdopodobienstwem " + comparisonLottery.P);
            Console.WriteLine(comparisonLottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - comparisonLottery.P) + "\n");

            Console.WriteLine("Wpisz '2' jeśli wolisz loterię:");
            Console.WriteLine(edgeValuesLottery.UpperUtilityValue.X + " z prawdopodobienstwem " + edgeValuesLottery.P);
            Console.WriteLine(edgeValuesLottery.LowerUtilityValue.X + " z prawdopodobienstwem " + (1 - edgeValuesLottery.P) + "\n");


            Console.WriteLine("Wpisz 'n' jeśli loterie są dla Ciebie nierozróżnialne\n");
            Console.WriteLine("'1', '2' lub 'n' :\n");

            return Console.ReadLine();
        }

        public float NewPointUtilityLotteries(Lottery edgeValuesLottery, Lottery comparisonLottery)
        {
            return edgeValuesLottery.P / comparisonLottery.P;
        }

        public void TalkToMeLotteriesComparison(Lottery edgeValuesLottery, Lottery comparisonLottery, float lowerUtilityBoundary, float upperUtilityBoundary)
        {
            string choice = displayChoicesLotteriesComparison(edgeValuesLottery, comparisonLottery);

            if (choice.Equals("1"))
            {
                lowerUtilityBoundary = edgeValuesLottery.P;
                edgeValuesLottery.P = (edgeValuesLottery.P + upperUtilityBoundary) / 2;
                TalkToMeLotteriesComparison(edgeValuesLottery, comparisonLottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
            else if (choice.Equals("2"))
            {
                upperUtilityBoundary = edgeValuesLottery.P;
                edgeValuesLottery.P = (edgeValuesLottery.P + lowerUtilityBoundary) / 2;
                TalkToMeLotteriesComparison(edgeValuesLottery, comparisonLottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy punkt!\n");
                PointsList.Add(new Point(comparisonLottery.UpperUtilityValue.X, NewPointUtilityLotteries(edgeValuesLottery, comparisonLottery)));
                PointsList.Sort((first, second) => first.X.CompareTo(second.X));
            }
            else
            {
                Console.WriteLine("\nPodano bledna wartosc!\n");
                TalkToMeLotteriesComparison(edgeValuesLottery, comparisonLottery, lowerUtilityBoundary, upperUtilityBoundary);
            }
        }

        public void CoefficientsDialog(List<Criterion> criterionList)
        {
            CriteriaCoefficientsList = new List<CriterionCoefficient>();
            var minValues = criterionList.Select(criterion => criterion.MinValue).ToArray();
            var maxValues = criterionList.Select(criterion => criterion.MaxValue).ToArray();
            var criterionNames = criterionList.Select(criterion => criterion.Name).ToArray();

            List<float> bestValues = new List<float>();
            List<float> worstValues = new List<float>();

            for (int i = 0; i < criterionList.Count; i++)
            {
                if (criterionList[i].CriterionDirection == "Cost")
                {
                    bestValues.Add(minValues[i]);
                    worstValues.Add(maxValues[i]);
                }
                else
                {
                    bestValues.Add(maxValues[i]);
                    worstValues.Add(minValues[i]);
                }
            }

            for (int i = 0; i < criterionList.Count; i++)
            {
                var valuesToCompare = worstValues.ToArray();
                valuesToCompare[i] = bestValues[i];
                var lotteryCoefficients = new LotteryCoefficients(criterionList, valuesToCompare, 0.5f);
                TalkToMeCoefficients(lotteryCoefficients, criterionNames[i], 0, 1);
            }


            for (int i = 0; i < criterionList.Count; i++)
                Console.WriteLine(CriteriaCoefficientsList[i].CriterionName + " : " + CriteriaCoefficientsList[i].Coefficient);
        }

        public string displayChoicesCoefficients(LotteryCoefficients lotteryCoefficients)
        {
            Console.WriteLine("Wpisz '1' jeśli wolisz WARIANT:");
            for (int i = 0; i < lotteryCoefficients.CriterionNames.Length; i++)
            {
                Console.WriteLine(lotteryCoefficients.CriterionNames[i] + " = " + lotteryCoefficients.ValuesToCompare[i]);
            }

            Console.WriteLine("\nWpisz '2' jeśli wolisz LOTERIĘ");
            for (int i = 0; i < lotteryCoefficients.CriterionNames.Length; i++)
            {
                Console.WriteLine(lotteryCoefficients.CriterionNames[i] + " = " + lotteryCoefficients.BestValues[i]);
            }

            Console.WriteLine("z prawdopodobienstwem " + lotteryCoefficients.P + "\n");
            
            for (int i = 0; i < lotteryCoefficients.CriterionNames.Length; i++)
            {
                Console.WriteLine(lotteryCoefficients.CriterionNames[i] + " = " + lotteryCoefficients.WorstValues[i]);
            }

            Console.WriteLine("z prawdopodobienstwem " + (1 - lotteryCoefficients.P) + "\n");

            Console.WriteLine("'1', '2' lub 'n' :\n");

            return Console.ReadLine();
        }

        public void TalkToMeCoefficients(LotteryCoefficients lotteryCoefficients, string currentCriterionName, float lowerProbabilityBoundary, float upperProbabilityBoundary)
        {
            string choice = displayChoicesCoefficients(lotteryCoefficients);

            if (choice.Equals("1"))
            {
                lowerProbabilityBoundary = lotteryCoefficients.P;
                lotteryCoefficients.P = (lowerProbabilityBoundary + upperProbabilityBoundary) / 2;
                TalkToMeCoefficients(lotteryCoefficients, currentCriterionName, lowerProbabilityBoundary, upperProbabilityBoundary);
            }
            else if (choice.Equals("2"))
            {
                upperProbabilityBoundary = lotteryCoefficients.P;
                lotteryCoefficients.P = (lowerProbabilityBoundary + upperProbabilityBoundary) / 2;
                TalkToMeCoefficients(lotteryCoefficients, currentCriterionName, lowerProbabilityBoundary, upperProbabilityBoundary);
            }
            else if (choice.Equals("n"))
            {
                Console.WriteLine("\nDodano nowy wspolczynnik!\n");
                CriteriaCoefficientsList.Add(new CriterionCoefficient(currentCriterionName, lotteryCoefficients.P));
            }
            else
            {
                Console.WriteLine("\nPodano bledna wartosc!\n");
                TalkToMeCoefficients(lotteryCoefficients, currentCriterionName, lowerProbabilityBoundary, upperProbabilityBoundary);
            }
        }
    }
}
