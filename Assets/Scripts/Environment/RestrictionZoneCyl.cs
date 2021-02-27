using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Not serializable. Holds current status of sub-cylinders within a stacked restriction zone.
    /// </summary>
    public class RestrictionZoneCyl : RestrictionZoneElemBase
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
                Scale = new Vector3(Scale.x, value / 2, Scale.z);
                Position = new Vector3(Position.x, _bottom + value / 2, Position.z);
            }
        }

        private float _radius = 1;
        public float Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                Scale = new Vector3(value, Scale.y, value);
            }
        }

        private float _bottom = 0;
        public float Bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                _bottom = value;
                Position = new Vector3(Position.x, value + Height / 2, Position.z);
            }
        }

        private float _top = 0;
        public float Top
        {
            get
            {
                return _top;
            }
            set
            {
                if (value > _bottom)
                {
                    _top = value;
                    Height = value - _bottom;
                }
                else
                {
                    Debug.LogError("Specified top elevation is below bottom elevation of this restriction zone");
                }
            }
        }

        public Vector3 Position { get; private set; } = new Vector3();
        public Vector3 Scale { get; private set; } = new Vector3();

        public RestrictionZoneCyl(float xPos, float zPos, float radius, float height, float bottom)
        {
            Position = new Vector3(xPos, 0, zPos);
            Radius = radius;
            Height = height;
            Bottom = bottom;
            _top = Bottom + height;
        }

        public void SetXZPos(float x, float z)
        {
            Position = new Vector3(x, Position.y, z);
        }
    }
}
