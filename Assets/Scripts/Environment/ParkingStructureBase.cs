using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public abstract class ParkingStructureBase
    {
        public int Layer { get; } = 11;

        public abstract string Type { get; set; }
        public abstract string Description { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 StandbyPosition { get; set; }//@Eunu comment
        public abstract Vector3 LandingQueueHead { get; set; }//@Eunu comment
        public abstract Vector3 LandingQueueDirection { get; set; }//@Eunu comment
        public abstract List<Vector3> ParkingSpots { get; set; }

        public abstract ParkingStructureBase GetCopy();

    }

}
