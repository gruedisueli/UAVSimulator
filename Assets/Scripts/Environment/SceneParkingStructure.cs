using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a parking structure in the scene itself.
    /// </summary>
    public class SceneParkingStructure
    {
        public GameObject SceneGameObject { get; set; }
        public ParkingStructureBase ParkingStructureSpecs { get; set; }

        public SceneParkingStructure(GameObject gO, ParkingStructureBase pS)
        {
            SceneGameObject = gO;
            ParkingStructureSpecs = pS;
        }
    }
}
