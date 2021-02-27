using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The very broad category of restriction zone. Not the specific type.
    /// </summary>
    public enum RestrictionZoneCategory
    {
        GenericRect,
        GenericCyl,
        Class,
        Other
    }

    [Serializable]
    public class RestrictionZone
    {
        [SerializeField]
        private RestrictionZoneCategory _category = RestrictionZoneCategory.Other;
        /// <summary>
        /// General category of zone.
        /// </summary>
        public RestrictionZoneCategory Category
        {
            get
            {
                return _category;
            }
        }

        [SerializeField]
        private string _type = "";
        /// <summary>
        /// Type as specified in file name of asset
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
        private Vector3 _position = new Vector3();
        /// <summary>
        /// Absolute position for generic rectangular restriction zones, xz position for cylindrical ones
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
        }

        [SerializeField]
        private Vector3 _rotation = new Vector3();
        /// <summary>
        /// Rotation is only applicable on generic rectangular restriction zones.
        /// </summary>
        public Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
        }

        [SerializeField]
        private Vector3 _scale = new Vector3();
        /// <summary>
        /// Scale is only applicable on generic rectangular restriction zones.
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
        }

        [SerializeField]
        private List<float> _stepElevs = new List<float>();
        /// <summary>
        /// Step elevs list is only applicable on cylindrical restriction zones.
        /// </summary>
        public List<float> StepElevs
        {
            get
            {
                return _stepElevs;
            }
        }

        [SerializeField]
        private List<float> _radii = new List<float>();
        /// <summary>
        /// Radii list is only applicable on cylindrical restriction zones
        /// </summary>
        public List<float> Radii
        {
            get
            {
                return _radii;
            }
        }

        [SerializeField]
        private float _height = 0;
        /// <summary>
        /// Height is only applicable on generic restriction zones.
        /// </summary>
        public float Height
        {
            get
            {
                return _height;
            }
        }

        public List<RestrictionZoneElemBase> Elements { get; private set; } = new List<RestrictionZoneElemBase>();

        public RestrictionZone()
        {

        }

        public RestrictionZone(RestrictionZone rZ)
        {
            _category = rZ.Category;
            _type = rZ.Type;
            _position = rZ.Position;
            _rotation = rZ.Rotation;
            _scale = rZ.Scale;
            _stepElevs = new List<float>(rZ.StepElevs);
            _radii = new List<float>(rZ.Radii);
            _height = rZ.Height;

            Initialize();
        }

        /// <summary>
        /// Call after deserializing json or instantiating new instance.
        /// </summary>
        public void Initialize()
        {
            if (Category == RestrictionZoneCategory.GenericRect)
            {
                Elements.Add(new RestrictionZoneRect(_position, _rotation, _scale.x, _scale.z, _height));
            }
            else if (Category == RestrictionZoneCategory.GenericCyl)
            {
                if (_radii != null && _stepElevs != null && _radii.Count == 1 && _stepElevs.Count == 1)
                {
                    Elements.Add(new RestrictionZoneCyl(_position.x, _position.z, _radii[0], _height, _stepElevs[0]));
                }
                else 
                {
                    Debug.LogError("Initialization Error: Generic cylinder restriction zone radius and step elevs lists should have length 1");
                }
            }
            else if (Category == RestrictionZoneCategory.Class)
            {
                if (_radii != null && _stepElevs != null && _radii.Count + 1 == _stepElevs.Count)
                {
                    for (int i = 0; i < _stepElevs.Count - 1; i++)
                    {
                        float bottom = _stepElevs[i];
                        float top = _stepElevs[i + 1];
                        float rad = _radii[i];
                        Elements.Add(new RestrictionZoneCyl(_position.x, _position.z, rad, top - bottom, bottom));
                    }
                }
                else
                {
                    Debug.LogError("Initialization Error: Generic class restriction zone radius list should have one more element than step elevs list");
                }
            }
            else
            {
                Debug.LogError("This category of restriction zone not yet supported");
            }
        }

        /// <summary>
        /// Sets height if this is a generic rectangular restriction zone.
        /// </summary>
        public void SetRectHeight(float h)
        {
            if (Category == RestrictionZoneCategory.GenericRect)
            {
                if (Elements != null && Elements.Count == 1 && Elements[0] is RestrictionZoneRect)
                {
                    var e = Elements[0] as RestrictionZoneRect;
                    e.Height = h;
                    _height = h;
                }
                else
                {
                    Debug.LogError("Element list not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.GenericRect.ToString()} restriction zone but this zone is not rectangular");
            }
        }

        /// <summary>
        /// Sets height if this is a generic cylindrical restriction zone.
        /// </summary>
        public void SetGenCylHeight(float h)
        {
            if (Category == RestrictionZoneCategory.GenericCyl)
            {
                if (Elements != null && Elements.Count == 1 && Elements[0] is RestrictionZoneCyl && _stepElevs != null && _stepElevs.Count == 2)
                {
                    var e = Elements[0] as RestrictionZoneCyl;
                    e.Height = h;
                    _height = h;
                    _stepElevs[1] = _stepElevs[0] + h;
                    _position = e.Position;
                }
                else
                {
                    Debug.LogError("Element list not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.GenericCyl.ToString()} restriction zone but this zone is not cylindrical");
            }
        }

        /// <summary>
        /// Sets elevation of specific step in class restriction zone.
        /// </summary>
        public void SetClassStepElev(float elev, int index)
        {
            if (Category == RestrictionZoneCategory.Class)
            {
                //update elements affected by this step
                if (Elements != null && Elements.Count > index && Elements[index] is RestrictionZoneCyl && _stepElevs != null && _stepElevs.Count > index)
                {
                    var e1 = Elements[index] as RestrictionZoneCyl;
                    var e0 = index > 0 ? Elements[index - 1] as RestrictionZoneCyl : null; //also affect element below, if applicable
                    e1.Bottom = elev;
                    if (e0 != null)
                    {
                        e0.Top = elev;
                    }
                    _stepElevs[index] = elev;
                }
                else
                {
                    Debug.LogError("Restriction zone not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.Class.ToString()} restriction zone but this zone is not class");
            }
        }

        /// <summary>
        /// Sets radius of generic cylindrical restriction zone.
        /// </summary>
        public void SetGenCylRadius(float radius)
        {
            if (Category == RestrictionZoneCategory.GenericCyl)
            {
                //update elements affected by this step
                if (Elements != null && Elements.Count == 1 && Elements[0] is RestrictionZoneCyl && _radii != null && _radii.Count == 1)
                {
                    var e1 = Elements[0] as RestrictionZoneCyl;
                    e1.Radius = radius;
                    _radii[0] = radius;
                }
                else
                {
                    Debug.LogError("Restriction zone not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.GenericCyl.ToString()} restriction zone but this zone is not class");
            }
        }

        /// <summary>
        /// Sets radius of specific step in class restriction zone.
        /// </summary>
        public void SetClassRadius(float radius, int index)
        {
            if (Category == RestrictionZoneCategory.Class)
            {
                //update elements affected by this step
                if (Elements != null && Elements.Count > index && Elements[index] is RestrictionZoneCyl && _radii != null && _radii.Count > index)
                {
                    var e1 = Elements[index] as RestrictionZoneCyl;
                    e1.Radius = radius;
                    _radii[index] = radius;
                }
                else
                {
                    Debug.LogError("Restriction zone not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.Class.ToString()} restriction zone but this zone is not class");
            }
        }


        /// <summary>
        /// Sets bottom elevation of generic cylindrical restriction zone.
        /// </summary>
        public void SetGenCylBotElev(float botElev)
        {
            if (Category == RestrictionZoneCategory.GenericCyl)
            {
                //update elements affected by this step
                if (Elements != null && Elements.Count == 1 && Elements[0] is RestrictionZoneCyl && _stepElevs != null && _stepElevs.Count == 2)
                {
                    var e1 = Elements[0] as RestrictionZoneCyl;
                    e1.Radius = radius;
                    _radii[0] = radius;
                }
                else
                {
                    Debug.LogError("Restriction zone not properly configured");
                }
            }
            else
            {
                Debug.LogError($"Attempting to change height of {RestrictionZoneCategory.GenericCyl.ToString()} restriction zone but this zone is not class");
            }
        }
    }
}
