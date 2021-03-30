using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

/// <summary>
/// A json-serializable version of Vector4
/// </summary>
namespace Assets.Scripts.Serialization
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerVect4f
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
        [JsonProperty]
        private float _z = 0;
        public float Z
        {
            get
            {
                return _z;
            }
            set
            {
                _z = value;
            }
        }
        [JsonProperty]
        private float _w = 0;
        public float W
        {
            get
            {
                return _w;
            }
            set
            {
                _w = value;
            }
        }

        public SerVect4f(Vector4 v)
        {
            _x = v.x;
            _y = v.y;
            _z = v.z;
            _w = v.w;
        }

        public SerVect4f(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public SerVect4f(SerVect4f v)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
            _w = v.W;
        }

        public SerVect4f()
        {

        }

        /// <summary>
        /// Returns a vector3 based on this object.
        /// </summary>
        public Vector4 ToVector4()
        {
            return new Vector4(_x, _y, _z, _w);
        }
    }
}
