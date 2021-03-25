using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public class DroneInstantiationArgs : System.EventArgs
    {
        public GameObject Drone { get; private set; }

        public DroneInstantiationArgs(GameObject drone)
        {
            Drone = drone;
        }
    }
}
