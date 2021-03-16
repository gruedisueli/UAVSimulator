using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.Utils;

using Newtonsoft.Json;

/// <summary>
/// A json-serializable version of a vector 2 (double) that Mapbox requires for certain operations.
/// </summary>
namespace Assets.Scripts.Serialization
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerVect2d
    {
        [JsonProperty]
        private double _x = 0;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        [JsonProperty]
        private double _y = 0;
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public SerVect2d(Vector2d v)
        {
            _x = v.x;
            _y = v.y;
        }

        public SerVect2d(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public SerVect2d()
        {

        }

        /// <summary>
        /// Returns a vector2d based on this object.
        /// </summary>
        public Vector2d ToVector2d()
        {
            return new Vector2d(_x, _y);
        }
    }
}
