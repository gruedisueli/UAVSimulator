using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class DronePortAssetPack
    {
        public GameObject Prefab { get; private set; } = null;
        public DronePortCustom Specs { get; private set; } = null;
        public Sprite PreviewImage { get; private set; } = null;

        public DronePortAssetPack(GameObject prefab, DronePortCustom specs, Sprite previewImage, string description)
        {
            Prefab = prefab;
            Specs = specs;
            PreviewImage = previewImage;
        }
    }
}
