using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Assets.Scripts.Serialization;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ParkingStructureCustom : ParkingStructureBase
    {
        [JsonProperty]
        private string _type = "";
        public override string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }


        [JsonProperty]
        private string _description = "";
        public override string Description
        {
            get
            {
                return _description;
            }
        }

        [JsonProperty]
        private int _remainingSpots = 0;
        public override int RemainingSpots
        {
            get
            {
                return _remainingSpots;
            }
            set
            {
                _remainingSpots = value;
            }
        }

        [JsonProperty]
        private SerVect3f _position = new SerVect3f();
        public override Vector3 Position
        {
            get
            {
                return _position.ToVector3();
            }
            set
            {
                _position = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _rotation = new SerVect3f();
        public override Vector3 Rotation
        {
            get
            {
                return _rotation.ToVector3();
            }
            set
            {
                _rotation = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _scale = new SerVect3f(1, 1, 1);
        public override Vector3 Scale
        {
            get
            {
                return _scale.ToVector3();
            }
            set
            {
                _scale = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _standbyPosition = new SerVect3f();
        public override Vector3 StandbyPosition
        {
            get
            {
                return _standbyPosition.ToVector3();
            }
            set
            {
                _standbyPosition = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _landingQueueHead = new SerVect3f();
        public override Vector3 LandingQueueHead
        {
            get
            {
                return _landingQueueHead.ToVector3();
            }
            set
            {
                _landingQueueHead = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _landingQueueDirection = new SerVect3f();
        public override Vector3 LandingQueueDirection
        {
            get
            {
                return _landingQueueDirection.ToVector3();
            }
            set
            {
                _landingQueueDirection = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private List<SerVect3f> _parkingSpots = new List<SerVect3f>();
        public override List<Vector3> ParkingSpots
        {
            get
            {
                List<Vector3> list = new List<Vector3>();
                foreach(var pS in _parkingSpots)
                {
                    list.Add(pS.ToVector3());
                }
                return list;
            }
            set
            {
                _parkingSpots = new List<SerVect3f>();
                foreach(var pS in value)
                {
                    _parkingSpots.Add(new SerVect3f(pS));
                }
            }
        }

        public override Dictionary<Vector3, GameObject> Parked { get; set; } = new Dictionary<Vector3, GameObject>();
        public override Dictionary<GameObject, Vector3> VehicleAt { get; set; } = new Dictionary<GameObject, Vector3>();
        public override Dictionary<GameObject, Vector3> Reserved { get; set; } = new Dictionary<GameObject, Vector3>();

        public ParkingStructureCustom()
        {

        }

        public ParkingStructureCustom(ParkingStructureCustom pS)
        {
            _type = pS.Type;
            _description = pS.Description;
            RemainingSpots = pS.RemainingSpots;
            Position = pS.Position;
            Rotation = pS.Rotation;
            Scale = pS.Scale;
            StandbyPosition = pS.StandbyPosition;
            LandingQueueHead = pS.LandingQueueHead;
            LandingQueueDirection = pS.LandingQueueDirection;
            ParkingSpots = new List<Vector3>(pS.ParkingSpots);
            RemainingSpots = ParkingSpots.Count;
        }

        public override ParkingStructureBase GetCopy()
        {
            return new ParkingStructureCustom(this);
        }
    }
}
