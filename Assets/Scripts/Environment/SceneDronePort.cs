using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a drone port in the scene itself.
    /// </summary>
    public class SceneDronePort
    {
        public GameObject SceneGameObject { get; set; }
        public DronePortBase DronePortSpecs { get; set; }

        public SceneDronePort(GameObject gO, DronePortBase dP)
        {
            SceneGameObject = gO;
            DronePortSpecs = dP;
        }
    }
}
