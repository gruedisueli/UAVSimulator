using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

/// <summary>
/// A json-serializable version of Vector3
/// </summary>
namespace Assets.Scripts.Serialization
{
    [Serializable]
    public class SerVect3f
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

        public SerVect3f(Vector3 v)
        {
            _x = v.x;
            _y = v.y;
            _z = v.z;
        }

        public SerVect3f(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public SerVect3f()
        {

        }

        /// <summary>
        /// Returns a vector3 based on this object.
        /// </summary>
        public Vector3 ToVector3()
        {
            return new Vector3(_x, _y, _z);
        }
    }
}
