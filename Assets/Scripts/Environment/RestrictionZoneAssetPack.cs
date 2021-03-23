using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class RestrictionZoneAssetPack : AssetPackBase
    {
        public GameObject Prefab { get; private set; } = null;
        public RestrictionZoneBase Specs { get; private set; } = null;
        public override Sprite PreviewImage { get; set; } = null;
        public RestrictionZoneCategory Category { get; private set; } = RestrictionZoneCategory.Unset;

        public RestrictionZoneAssetPack(GameObject prefab, RestrictionZoneBase specs, Sprite previewImage, RestrictionZoneCategory category)
        {
            Prefab = prefab;
            Specs = specs;
            PreviewImage = previewImage;
            Category = category;
        }
    }
}
