using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Events;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class RestrictionZoneCyl : RestrictionZoneBase
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
                _scale = new Vector3(Scale.x, value / 2, Scale.z);
                _position = new Vector3(Position.x, _bottom + value / 2, Position.z);
            }
        }

        [SerializeField]
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
                _scale = new Vector3(value, Scale.y, value);
            }
        }

        [SerializeField]
        private float _bottom = 0;
        public float Bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                if (value < _top)
                {
                    _bottom = value;
                    _position = new Vector3(Position.x, value + Height / 2, Position.z);
                }
                else
                {
                    Debug.LogError("Specified bottom elevation is above top elevation of this restriction zone");
                }
            }
        }

        [SerializeField]
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
                    _height = value - _bottom;
                    _scale = new Vector3(Scale.x, _height / 2, Scale.z);
                    _position = new Vector3(Position.x, _bottom + _height / 2, Position.z);
                }
                else
                {
                    Debug.LogError("Specified top elevation is below bottom elevation of this restriction zone");
                }
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

        [SerializeField]
        private Vector3 _scale = new Vector3();
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
        }

        public RestrictionZoneCyl(Vector3 pos)
        {
            _position = pos;
        }

        public RestrictionZoneCyl(RestrictionZoneCyl rZ)
        {
            _type = rZ.Type;
            _height = rZ.Height;
            _radius = rZ.Radius;
            _bottom = rZ.Bottom;
            _top = rZ.Top;
            _position = rZ.Position;
            _rotation = rZ.Rotation;
            _scale = rZ.Scale;
        }

        public override RestrictionZoneBase GetCopy()
        {
            return new RestrictionZoneCyl(this);
        }

        public void SetXZPos(Vector3 xz)
        {
            _position = new Vector3(xz.x, Position.y, xz.z);
        }

        public override void UpdateParams(UpdatePropertyArgBase args)
        {
            try
            {
                switch (args.Type)
                {
                    //case UpdatePropertyType.Type:
                    //    {

                    //    }
                    case UpdatePropertyType.Height:
                        {
                            Height = (args as UpdateFloatPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Radius:
                        {
                            Radius = (args as UpdateFloatPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Bottom:
                        {
                            Bottom = (args as UpdateFloatPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Top:
                        {
                            Top = (args as UpdateFloatPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.XZPosition:
                        {
                            SetXZPos((args as UpdateVector3PropertyArg).Value);
                            break;
                        }
                    case UpdatePropertyType.Rotation:
                        {
                            Rotation = (args as UpdateVector3PropertyArg).Value;
                            break;
                        }
                }
            }
            catch
            {
                Debug.LogError("Casting error in restriction zone property update");
                return;
            }
        }
    }
}
