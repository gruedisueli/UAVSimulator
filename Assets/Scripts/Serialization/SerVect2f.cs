using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

/// <summary>
/// A json-serializable version of Vector2
/// </summary>
namespace Assets.Scripts.Serialization
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerVect2f
    {
        [JsonProperty]
        private float _x = 0;
        public float X
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
        private float _y = 0;
        public float Y
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

        public SerVect2f(Vector2 v)
        {
            _x = v.x;
            _y = v.y;
        }

        public SerVect2f(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public SerVect2f(SerVect2f v)
        {
            _x = v.X;
            _y = v.Y;
        }

        public SerVect2f()
        {

        }

        /// <summary>
        /// Returns a vector3 based on this object.
        /// </summary>
        public Vector2 ToVector2()
        {
            return new Vector2(_x, _y);
        }
    }
}
