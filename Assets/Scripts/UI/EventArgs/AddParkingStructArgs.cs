using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public class AddParkingStructArgs : IAddElementArgs
    {
        public ParkingStructCategory Category { get; private set; }
        public string Type { get; private set; }
        public Vector3 Position { get; private set; }

        public AddParkingStructArgs(ParkingStructCategory category, string type, Vector3 pos)
        {
            Category = category;
            Type = type;
            Position = pos;
        }
    }
}
