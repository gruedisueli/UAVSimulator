using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class RestrictionZoneRect : RestrictionZoneElemBase
    {
        private float _height = 1;
        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                Scale = new Vector3(Scale.x, _height, Scale.z);
            }
        }

        public Vector3 Scale { get; private set; } = new Vector3();
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public RestrictionZoneRect(Vector3 pos, Vector3 rot, float xScale, float zScale, float height)
        {
            Position = pos;
            Rotation = rot;
            Scale = new Vector3(xScale, 0, zScale);
            Height = height;
        }

        public void SetXScale(float x)
        {
            Scale = new Vector3(x, Scale.y, Scale.z);
        }

        public void SetZScale(float z)
        {
            Scale = new Vector3(Scale.x, Scale.y, z);
        }
    }
}
