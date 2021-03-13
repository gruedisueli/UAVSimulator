using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public class AddCityArgs : IAddElementArgs
    {
        public string Type { get; private set; } = "";
        public Vector3 Position { get; private set; }
        public RaycastHit HitInfo { get; private set; }

        public AddCityArgs(Vector3 pos, RaycastHit hitInfo)
        {
            Position = pos;
            HitInfo = hitInfo;
        }
    }
}
