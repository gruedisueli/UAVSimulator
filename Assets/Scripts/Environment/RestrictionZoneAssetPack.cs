using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class RestrictionZoneAssetPack
    {
        public GameObject Prefab { get; private set; }
        public RestrictionZone Specs { get; private set; }

        public RestrictionZoneAssetPack(GameObject prefab, RestrictionZone specs)
        {
            Prefab = prefab;
            Specs = specs;
        }
    }
}
