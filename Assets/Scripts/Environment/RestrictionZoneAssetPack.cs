using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class RestrictionZoneAssetPack
    {
        public GameObject Prefab { get; private set; } = null;
        public RestrictionZoneBase Specs { get; private set; } = null;
        public Sprite PreviewImage { get; private set; } = null;

        public RestrictionZoneAssetPack(GameObject prefab, RestrictionZoneBase specs, Sprite previewImage)
        {
            Prefab = prefab;
            Specs = specs;
            PreviewImage = previewImage;
        }
    }
}
