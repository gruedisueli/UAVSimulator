using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public class AddRegionArgs : IAddElementArgs
    {
        public string Type { get; } = "";
        public Vector3 Position { get; private set; }
        public RaycastHit HitInfo { get; private set; }

        public AddRegionArgs(Vector3 pos, RaycastHit hit)
        {
            Position = pos;
            HitInfo = hit;
        }

    }
}
