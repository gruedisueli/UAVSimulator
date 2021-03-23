using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class ParkingStructureAssetPack : AssetPackBase
    {
        public GameObject Prefab { get; private set; } = null;
        public ParkingStructureCustom Specs { get; private set; } = null;
        public override Sprite PreviewImage { get; set; } = null;
        public ParkingStructCategory Category { get; private set; } = ParkingStructCategory.Unset;

        public ParkingStructureAssetPack(GameObject prefab, ParkingStructureCustom specs, Sprite previewImage, ParkingStructCategory category)
        {
            Prefab = prefab;
            Specs = specs;
            PreviewImage = previewImage;
            Category = category;
        }
    }
}
