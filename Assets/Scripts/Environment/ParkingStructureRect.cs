using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class ParkingStructureRect : ParkingStructureBase
    {
        [SerializeField]
        private string _type = "";
        public override string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
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

        [SerializeField]
        private Vector3 _position = new Vector3();
        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        [SerializeField]
        private Vector3 _rotation = new Vector3();
        public override Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        [SerializeField]
        private Vector3 _scale = new Vector3();
        public override Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        [SerializeField]
        private Vector3 _standbyPosition = new Vector3();
        public override Vector3 StandbyPosition
        {
            get
            {
                return _standbyPosition;
            }
            set
            {
                _standbyPosition = value;
            }
        }

        [SerializeField]
        private Vector3 _landingQueueHead = new Vector3();
        public override Vector3 LandingQueueHead
        {
            get
            {
                return _landingQueueHead;
            }
            set
            {
                _landingQueueHead = value;
            }
        }

        [SerializeField]
        private Vector3 _landingQueueDirection = new Vector3();
        public override Vector3 LandingQueueDirection
        {
            get
            {
                return _landingQueueDirection;
            }
            set
            {
                _landingQueueDirection = value;
            }
        }

        [SerializeField]
        private List<Vector3> _parkingSpots = new List<Vector3>();
        public override List<Vector3> ParkingSpots
        {
            get
            {
                return _parkingSpots;
            }
            set
            {
                _parkingSpots = value;
            }
        }

        public override Dictionary<Vector3, GameObject> Parked { get; set; } = new Dictionary<Vector3, GameObject>();
        public override Dictionary<GameObject, Vector3> VehicleAt { get; set; } = new Dictionary<GameObject, Vector3>();
        public override Dictionary<GameObject, Vector3> Reserved { get; set; } = new Dictionary<GameObject, Vector3>();

        public ParkingStructureRect(Vector3 pos)
        {
            Position = pos;
        }

        public ParkingStructureRect(ParkingStructureRect pS)
        {
            _type = pS.Type;
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
            return new ParkingStructureRect(this);
        }

        /// <summary>
        /// Applies a parking grid to current configuration of structure, using predefined rules. REMOVES any existing parking spots and rebuilds list.
        /// </summary>
        public void ApplyParkingGrid()
        {
            //TO-DO: Use real numbers for margins - now 20m
            ParkingSpots = new List<Vector3>();
            int parkingMargin = 20;
            for (int i = (int)(-(Scale.x / 2)) + parkingMargin; i <= (int)(Scale.x / 2) - parkingMargin; i += parkingMargin)
            {
                for (int j = (int)(-(Scale.z / 2)) + parkingMargin; j <= (int)(Scale.z / 2) - parkingMargin; j += parkingMargin)
                {
                    Vector3 v = new Vector3((float)i, 0.0f, (float)j);
                    ParkingSpots.Add(v);
                }
            }
            RemainingSpots = ParkingSpots.Count;
        }
    }
}
