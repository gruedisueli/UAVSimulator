using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class RestrictionZone
    {
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public List<float> bottoms;
        public List<float> radius;
        public float height;
    }
}
