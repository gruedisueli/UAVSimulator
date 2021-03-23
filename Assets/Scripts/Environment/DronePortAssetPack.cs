using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class DronePortAssetPack : AssetPackBase
    {
        public GameObject Prefab { get; private set; } = null;
        public DronePortCustom Specs { get; private set; } = null;
        public override Sprite PreviewImage { get; set; } = null;
        public DronePortCategory Category { get; private set; } = DronePortCategory.Unset;

        public DronePortAssetPack(GameObject prefab, DronePortCustom specs, Sprite previewImage, DronePortCategory category)
        {
            Prefab = prefab;
            Specs = specs;
            PreviewImage = previewImage;
            Category = category;
        }
    }
}
