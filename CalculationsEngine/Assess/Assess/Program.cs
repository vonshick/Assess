using System;
using System.Collections.Generic;
using DataModel.Input;

namespace CalculationsEngine.Assess.Assess
{
    class Program
    {
        private static List<Criterion> createSampleCriteria()
        {
            List<Criterion> sampleCriteriaList = new List<Criterion>();
//            sampleCriteriaList.Add(new Criterion("CONS", """c", 6.1f, 9.7f));
//            sampleCriteriaList.Add(new Criterion("SPACE", "g", 4.93f, 8.67f));
//            sampleCriteriaList.Add(new Criterion("PRICE", "c", 28.8f, 72.17f));
//            sampleCriteriaList.Add(new Criterion("SPEED", "g", 110f, 190f));

            return sampleCriteriaList;
        }

        static void countKCoefficient(List<CriterionCoefficient> criteriaCoefficientsList)
        {
            // K + 1 = ILOCZYN PO KRYTERIACH ( K * k_i + 1)
        }

        static void Main(string[] args)
        {
            // var sampleCriteriaList = createSampleCriteria();

            // //            CoefficientsDialog coefficientsDialog = new CoefficientsDialog(sampleCriteriaList);
            // //            coefficientsDialog.GetCoefficientsForCriteria();

            // // countKCoefficient(assessCoefficientsDialog.CriteriaCoefficientsList);

            // DialogController dialogController = new DialogController(sampleCriteriaList[0], 2, 0.3f);

            // dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);

            // Console.ReadLine();



            //check how Baristow solver works
            var coefficients = new double[]
            {
//                0,
//                0.25 + 0.5 + 0.75 + 0.875 - 1,
//                0.25 * (0.5 + 0.75 + 0.875) + 0.5 * (0.75 + 0.875) + 0.75 * 0.875,
//                0.25*0.5*0.75 + 0.25*0.5*0.875 + 0.25*0.75*0.875 + 0.5*0.75*0.875,
//                0.25 * 0.5 * 0.75 * 0.875

                0,
                0.25 + 0.5 + 0.5 + 0.5 - 1,
                0.25 * (0.5 + 0.5 + 0.5) + 0.5 * (0.5 + 0.5) + 0.5 * 0.5,
                0.25*0.5*0.5 + 0.25*0.5*0.5 + 0.25*0.5*0.5 + 0.5*0.5*0.5,
                0.25 * 0.5 * 0.5 * 0.5

//                0,
//                0.375 + 0.5 + 0.5 + 0.5 - 1,
//                0.375 * (0.5 + 0.5 + 0.5) + 0.5 * (0.5 + 0.5) + 0.5 * 0.5,
//                0.375*0.5*0.5 + 0.375*0.5*0.5 + 0.375*0.5*0.5 + 0.5*0.5*0.5,
//                0.375 * 0.5 * 0.5 * 0.5

//
//                1024, 3840, 5760, 4320, 1620, 243
            };

            BaristowSolver baristow = new BaristowSolver(coefficients);
            
            double K = baristow.GetScallingCoefficient();

            Console.ReadLine();
        }
    }
}
