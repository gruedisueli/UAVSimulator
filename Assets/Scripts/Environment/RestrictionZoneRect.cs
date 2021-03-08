using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class RestrictionZoneRect : RestrictionZoneBase
    {
        [SerializeField]
        private string _type = "";
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
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
                _scale = new Vector3(Scale.x, _height, Scale.z);
            }
        }

        [SerializeField]
        private Vector3 _scale = new Vector3(1, 0, 1);
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
        }

        [SerializeField]
        private Vector3 _position = new Vector3();
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        [SerializeField]
        private Vector3 _rotation = new Vector3();
        public Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        public RestrictionZoneRect(Vector3 pos)
        {
            Position = pos;
        }

        public RestrictionZoneRect(RestrictionZoneRect rZ)
        {
            _type = rZ.Type;
            _height = rZ.Height;
            _scale = rZ.Scale;
            _position = rZ.Position;
            _rotation = rZ.Rotation;
        }

        public void SetXScale(float x)
        {
            _scale = new Vector3(x, Scale.y, Scale.z);
        }

        public void SetZScale(float z)
        {
            _scale = new Vector3(Scale.x, Scale.y, z);
        }
    }
}
