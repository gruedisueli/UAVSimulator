using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.Tools;
using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public class DroneInstantiationArgs : System.EventArgs
    {
        public DroneIcon Icon { get; private set; }

        public DroneInstantiationArgs(DroneIcon icon)
        {
            Icon = icon;
        }
    }
}
