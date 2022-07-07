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
    public class RestrictionZoneRect : RestrictionZoneBase
    {
        private static readonly float DEFAULT_EDGE_LENGTH = 100;

        [JsonProperty]
        private string _type = "Rect";
        public override string Type
        {
            get
            {
                return _type;
            }
        }


        [JsonProperty]
        private string _description = "A basic rectangular restriction zone";
        public override string Description
        {
            get
            {
                return _description;
            }
        }

        [JsonProperty]
        private float _height = DEFAULT_EDGE_LENGTH;
        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                float diff = value - _height;
                _height = value;
                _scale = new SerVect3f(Scale.x, _height, Scale.z);
                _position = new SerVect3f(Position.x, Position.y + diff/2, Position.z);
            }
        }

        [JsonProperty]
        private SerVect3f _scale = new SerVect3f(DEFAULT_EDGE_LENGTH, DEFAULT_EDGE_LENGTH, DEFAULT_EDGE_LENGTH);
        public Vector3 Scale
        {
            get
            {
                return _scale.ToVector3();
            }
        }

        [JsonProperty]
        private SerVect3f _position = new SerVect3f();
        public override Vector3 Position
        {
            get
            {
                return _position.ToVector3();
            }
            set
            {
                _position = new SerVect3f(value);
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

        /// <summary>
        /// Empty constructor for json deserialization
        /// </summary>
        public RestrictionZoneRect()
        {

        }

        public RestrictionZoneRect(Vector3 pos)
        {
            _position = new SerVect3f(pos.x, pos.y + _height / 2, pos.z);
        }

        public RestrictionZoneRect(RestrictionZoneRect rZ)
        {
            _type = rZ.Type;
            _description = rZ.Description;
            _height = rZ.Height;
            _scale = new SerVect3f(rZ.Scale);
            _position = new SerVect3f(rZ.Position);
            _rotation = new SerVect3f(rZ.Rotation);
        }

        public override RestrictionZoneBase GetCopy()
        {
            return new RestrictionZoneRect(this);
        }

        public override List<Vector3> GetBoundaryPtsAtHeight(float height, float inflation)
        {
            float width = _scale.X;
            float depth = _scale.Z;
            float h = (float) Math.Sqrt(Math.Pow(width / 2, 2) + Math.Pow(depth / 2, 2));//hypotenuse
            float r = h + inflation;//radius of circle that inscribes the inflated rectangle
            float scaleFactor = r / h;
            width *= scaleFactor;
            depth *= scaleFactor;
            Vector2[] corners = {new Vector2(width / 2, depth / 2), new Vector2(width / 2 * -1, depth / 2), new Vector2(width / 2 * -1, depth / 2 * -1), new Vector2(width / 2, depth / 2 * -1)};
            var rotRad = (float)(Math.PI / 180) * _rotation.Y;
            var rotCorners = new List<Vector3>();
            for (int i = 0; i < 4; i++)
            {
                var c = new Vector3(_position.X + corners[i].x, height, _position.Z + corners[i].y);
                var dir = c - Position;
                dir = Quaternion.Euler(0, _rotation.Y, 0) * dir;
                c = Position + dir;
                rotCorners.Add(c);

                //double a = (double) rotRad + Math.Asin((double) (c.y / c.x));
                //float x = _position.X + (float)Math.Cos(a) * r;
                //float z = _position.Z + (float)Math.Sin(a) * r;
                //rotCorners.Add(new Vector3(x, height, z));
            }

            return rotCorners;
        }

        public void SetXScale(float x)
        {
            _scale = new SerVect3f(x, Scale.y, Scale.z);
        }

        public void SetZScale(float z)
        {
            _scale = new SerVect3f(Scale.x, Scale.y, z);
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
                    case ElementPropertyType.Height:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                Height = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                            }
                            break;
                        }
                    case ElementPropertyType.XScale:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                var sX = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                                SetXScale(sX);
                            }
                            break;
                        }
                    case ElementPropertyType.ZScale:
                        {
                            if (args is ModifyFloatPropertyArg fP)
                            {
                                var sZ = isMetric ? fP.Value : UnitUtils.FeetToMeters(fP.Value);
                                SetZScale(sZ);
                            }
                            break;
                        }
                    case ElementPropertyType.Position:
                        {
                            SetXZPos((args as ModifyVector3PropertyArg).Value);
                            break;
                        }
                    case ElementPropertyType.Rotation:
                        {
                            Rotation = (args as ModifyVector3PropertyArg).Value;
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
