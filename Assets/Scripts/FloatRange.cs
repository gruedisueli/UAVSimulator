using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class FloatRange
    {
        public float Max { get; }
        public float Min { get; }

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Returns value, clamped within this range of values.
        /// </summary>
        public float ClampToRange(float v)
        {
            if (v <= Max && v >= Min)
            {
                return v;
            }

            return v > Max ? Max : Min;
        }
    }
}
