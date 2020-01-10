using System;
using System.Collections.Generic;

namespace CalculationsEngine.Assess.Assess
{
    class Program
    {
        private static List<Criterion> createSampleCriteria()
        {
            List<Criterion> sampleCriteriaList = new List<Criterion>();
            sampleCriteriaList.Add(new Criterion("CONS", "c", 6.1f, 9.7f));
            sampleCriteriaList.Add(new Criterion("SPACE", "g", 4.93f, 8.67f));
            sampleCriteriaList.Add(new Criterion("PRICE", "c", 28.8f, 72.17f));
            sampleCriteriaList.Add(new Criterion("SPEED", "g", 110f, 190f));

            return sampleCriteriaList;
        }

        static void countKCoefficient(List<CriterionCoefficient> criteriaCoefficientsList)
        {
            // K + 1 = ILOCZYN PO KRYTERIACH ( K * k_i + 1)
        }

        static void Main(string[] args)
        {
            var sampleCriteriaList = createSampleCriteria();

            //            CoefficientsDialog coefficientsDialog = new CoefficientsDialog(sampleCriteriaList);
            //            coefficientsDialog.GetCoefficientsForCriteria();

            // countKCoefficient(assessCoefficientsDialog.CriteriaCoefficientsList);

            DialogController dialogController = new DialogController(sampleCriteriaList[0], 2, 0.3f);

            dialogController.TriggerDialog(dialogController.PointsList[0], dialogController.PointsList[1]);

            Console.ReadLine();
        }
    }
}
