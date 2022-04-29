using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class IntRange
    {
        public int Max { get; }
        public int Min { get; }

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Returns value, clamped within this range of values.
        /// </summary>
        public int ClampToRange(int v)
        {
            if (v <= Max && v >= Min)
            {
                return v;
            }

            return v > Max ? Max : Min;
        }
    }
}
