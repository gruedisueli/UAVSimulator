using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class ParkingStructureAssetPack
    {
        public GameObject Prefab { get; private set; }
        public ParkingStructure Specs { get; private set; }

        public ParkingStructureAssetPack(GameObject prefab, ParkingStructure specs)
        {
            Prefab = prefab;
            Specs = specs;
        }
    }
}
