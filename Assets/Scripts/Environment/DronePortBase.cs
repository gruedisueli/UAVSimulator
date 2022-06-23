using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public abstract class DronePortBase
    {
        public int Layer { get; } = 12;

        public abstract string Type { get; }
        public abstract string Description { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 StandbyPosition { get; set; }
        public abstract Vector3 LandingQueueHead { get; set; }
        public abstract Vector3 LandingQueueDirection { get; set; }
        public abstract Vector3 LandingPoint { get; set; }
        public abstract float MaximumVehicleSize { get; set; }
        public abstract bool IsMountable { get; set; }
        public abstract bool IsOnTheGround { get; set; }
        public abstract bool IsScalable { get; set; }
        public abstract DronePortBase GetCopy();

        // Translates to the global coordinate
        public Vector3 TranslateLandingGuidePosition(Vector3 parkingSpot)
        {
            return (Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z) * parkingSpot + Position);
        }

        public List<Vector3> GetLandingGuide(string mode)
        {
            List<Vector3> guides = new List<Vector3>();

            guides.Add(StandbyPosition);
            guides.Add(LandingPoint);

            if (mode == "takeoff")
            {
                guides.Reverse();
            }

            return guides;
        }
    }
}
