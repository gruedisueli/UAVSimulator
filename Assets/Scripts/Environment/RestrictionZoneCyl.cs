using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RestrictionZoneCyl : RestrictionZoneBase
    {
        private static readonly float DEFAULT_HEIGHT = 1000;
        private static readonly float DEFAULT_RADIUS = 250;

        [JsonProperty]
        private string _type = "Cyl";
        public override string Type
        {
            get
            {
                return _type;
            }
        }


        [JsonProperty]
        private string _description = "A basic cylindrical restriction zone";
        public override string Description
        {
            get
            {
                return _description;
            }
        }

        [JsonProperty]
        private float _height = DEFAULT_HEIGHT;
        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                _scale = new SerVect3f(Scale.x, value / 2, Scale.z);
                _position = new SerVect3f(Position.x, _bottom + value / 2, Position.z);
            }
        }

        [JsonProperty]
        private float _radius = DEFAULT_RADIUS;
        public float Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                _scale = new SerVect3f(value, Scale.y, value);
            }
        }

        [JsonProperty]
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
                    _height = _top - _bottom;
                    _scale = new SerVect3f(Scale.x, _height / 2, Scale.z);
                    _position = new SerVect3f(Position.x, value + Height / 2, Position.z);
                }
                else
                {
                    Debug.LogError("Specified bottom elevation is above top elevation of this restriction zone");
                }
            }
        }

        [JsonProperty]
        private float _top = DEFAULT_HEIGHT;
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
                    _scale = new SerVect3f(Scale.x, _height / 2, Scale.z);
                    _position = new SerVect3f(Position.x, _bottom + _height / 2, Position.z);
                }
                else
                {
                    Debug.LogError("Specified top elevation is below bottom elevation of this restriction zone");
                }
            }
        }

        [JsonProperty]
        private SerVect3f _position = new SerVect3f();
        public Vector3 Position
        {
            get
            {
                return _position.ToVector3();
            }
        }

        [JsonProperty]
        private SerVect3f _rotation = new SerVect3f();
        public Vector3 Rotation
        {
            get
            {
                return _rotation.ToVector3();
            }
            set
            {
                _rotation = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _scale = new SerVect3f(DEFAULT_RADIUS, DEFAULT_HEIGHT, DEFAULT_RADIUS);
        public Vector3 Scale
        {
            get
            {
                return _scale.ToVector3();
            }
        }

        /// <summary>
        /// Empty constructor for json deserialization
        /// </summary>
        public RestrictionZoneCyl()
        {

        }

        public RestrictionZoneCyl(Vector3 centerPt, float bottom, float top, float radius)
        {
            _position = new SerVect3f(centerPt);

            //Important: update properties from "set", and not on fields directly because this will also update other properties affected by these changes.
            Top = top;//set top before bottom
            Bottom = bottom;//set bottom after top
            Radius = radius;
        }

        public RestrictionZoneCyl(Vector3 pos)
        {
            _position = new SerVect3f(pos);
            Top = pos.y + _height;
            Bottom = pos.y;
        }

        public RestrictionZoneCyl(RestrictionZoneCyl rZ)
        {
            _type = rZ.Type;
            _description = rZ.Description;
            _height = rZ.Height;
            _radius = rZ.Radius;
            _bottom = rZ.Bottom;
            _top = rZ.Top;
            _position = new SerVect3f(rZ.Position);
            _rotation = new SerVect3f(rZ.Rotation);
            _scale = new SerVect3f(rZ.Scale);
        }

        public override RestrictionZoneBase GetCopy()
        {
            return new RestrictionZoneCyl(this);
        }

        public void SetXZPos(Vector3 xz)
        {
            _position = new SerVect3f(xz.x, Position.y, xz.z);
        }

        public override void UpdateParams(ModifyPropertyArgBase args)
        {
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            try
            {
                switch (args.ElementPropertyType)
                {
                    //case UpdatePropertyType.Type:
                    //    {

                    //    }
                    case ElementPropertyType.Height:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                Height = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                            }
                            break;
                        }
                    case ElementPropertyType.Radius:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                Radius = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                            }
                            break;
                        }
                    case ElementPropertyType.Bottom:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                Bottom = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                            }
                            break;
                        }
                    case ElementPropertyType.Top:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                Top = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                            }
                            break;
                        }
                    //case ElementPropertyType.XZPosition:
                    //    {
                    //        SetXZPos((args as ModifyVector3PropertyArg).Value);
                    //        break;
                    //    }
                    //case ElementPropertyType.Rotation:
                    //    {
                    //        Rotation = (args as ModifyVector3PropertyArg).Value;
                    //        break;
                    //    }
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
