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
        public RestrictionZoneCategory category = RestrictionZoneCategory.Other;
        public string type = "";
        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3();
        public Vector3 scale = new Vector3();
        public List<float> bottoms = new List<float>();
        public List<float> radius = new List<float>();
        public float height = 0;

        public RestrictionZone()
        {

        }

        public RestrictionZone(RestrictionZone rZ)
        {
            category = rZ.category;
            type = rZ.type;
            position = rZ.position;
            rotation = rZ.rotation;
            scale = rZ.scale;
            bottoms = new List<float>(rZ.bottoms);
            radius = new List<float>(rZ.radius);
            height = rZ.height;
        }

        /// <summary>
        /// Updates height on this zone.
        /// </summary>
        public void SetHeight(float h)
        {
            height = h;
            if (category == RestrictionZoneCategory.GenericRect)
            {
                scale = new Vector3(scale.x, height, scale.z);
            }
            else if (category == RestrictionZoneCategory.GenericCyl)
            {
                scale = new Vector3(scale.x, height / 2, scale.z);
            }
            else if (category == RestrictionZoneCategory.Class && bottoms != null && radius != null)
            {
                for (int i = 0; i < bottoms.Count - 1; i++)
                {
                    var prms = GetClassCylPrms(i);
                    if (prms != null)
                    {
                        float this_cylinder_center_y = prms[0];
                        float this_cylinder_height_half = prms[1];
                        float this_cylinder_radius = prms[2];
                        position = new Vector3(position.x, this_cylinder_center_y, position.z);
                        scale = new Vector3(this_cylinder_radius, this_cylinder_height_half, this_cylinder_radius);
                    }
                }
            }
        }

        /// <summary>
        /// Updates scale on this zone.
        /// </summary>
        public void SetScale(Vector3 s)
        {
            scale = s;
            if (category == RestrictionZoneCategory.GenericRect)
            {
                height = s.y;
            }
            else if (category == RestrictionZoneCategory.GenericCyl)
            {
                height = s.y * 2;
            }
            else if (category == RestrictionZoneCategory.Class && bottoms != null && radius != null)
            {
                for (int i = 0; i < bottoms.Count - 1; i++)
                {

                }
            }
        }

        /// <summary>
        /// Returns general params for class cylinder. Null on failure.
        /// </summary>
        private float[] GetClassCylPrms(int i)
        {
            if (bottoms.Count > i + 1)
            {
                float this_cylinder_center_y = (bottoms[i] + bottoms[i + 1]) / 2.0f;
                float this_cylinder_height_half = (bottoms[i + 1] - bottoms[i]) / 2.0f;
                float this_cylinder_radius = radius[i];
                return new float[] { this_cylinder_center_y, this_cylinder_height_half, this_cylinder_radius };
            }
            else
            {
                Debug.Log("Unable to get center y of restriction zone");
                return null;
            }
        }

    }
}
