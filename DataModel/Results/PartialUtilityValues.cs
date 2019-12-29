using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DataModel.Results
{
    public class PartialUtilityValues
    {
        public float Point;
        public float Value;
        public float MinValue;
        public float MaxValue;

        public PartialUtilityValues(float point, float value)
        {
            Point = point;
            Value = value;
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
        }

        public PartialUtilityValues(float point, float value, float minValue, float maxValue)
        {
            Point = point;
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
