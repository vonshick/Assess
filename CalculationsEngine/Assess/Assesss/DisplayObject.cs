using System.Collections.Generic;

namespace Assess
{
    public class DisplayObject
    {
        public DisplayObject()
        {
            PointsList = new List<Point>();
        }

        public float X;
        public float P; //lotteries comparison
        public Lottery Lottery;
        public List<Point> PointsList;
        public Lottery EdgeValuesLottery; //lotteries comparison
        public Lottery ComparisonLottery; //lotteries comparison
    }

}