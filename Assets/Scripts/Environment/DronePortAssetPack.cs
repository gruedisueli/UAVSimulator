using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class DronePortAssetPack
    {
        public GameObject Prefab { get; private set; } = null;
        public DronePort Specs { get; private set; } = null;

        public DronePortAssetPack(GameObject prefab, DronePort specs)
        {
            Prefab = prefab;
            Specs = specs;
        }
    }
}
