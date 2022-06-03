using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ParkingStructureBase
    {
        public int Layer { get; } = 11;

        public abstract string Type { get; set; }
        public abstract string Description { get; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 StandbyPosition { get; set; }
        public abstract Vector3 LandingQueueHead { get; set; }
        public abstract Vector3 LandingQueueDirection { get; set; }
        public abstract List<Vector3> ParkingSpots { get; set; }
        public abstract ParkingStructureBase GetCopy();

        [JsonProperty]
        protected int _droneInstantiationCt = -1;
        [JsonProperty]
        protected int _bufferParkingSpots = 3;

        public void ResetDroneInstantiationCt()
        {
            _droneInstantiationCt = ParkingSpots.Count - _bufferParkingSpots;
        }

        /// <summary>
        /// Returns the number of drones that should be instantiated for this structure at runtime.
        /// </summary>
        public int GetDroneInstantiationCt()
        {
            if (_droneInstantiationCt == -1)
            {
                ResetDroneInstantiationCt();
            }

            return _droneInstantiationCt;
        }

        /// <summary>
        /// Tries to set the number of drones that should be instantiated for this structure at runtime. Returns true if number is valid, false if not (outside of range). In case of out-of-range, sets value to max or min value.
        /// </summary>
        public bool TrySetDroneInstantiationCt(int ct)
        {
            if (ct < ParkingSpots.Count - _bufferParkingSpots && ct > 0)
            {
                _droneInstantiationCt = ct;
                return true;
            }
            
            if (ct < 1)
            {
                _droneInstantiationCt = 1;
            }
            else
            {
                _droneInstantiationCt = ParkingSpots.Count - _bufferParkingSpots;
            }

            return false;
        }

    }

}
