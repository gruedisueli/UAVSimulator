using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public enum ParkingStructureCategory
    {
        Scalable,
        Fixed
    }

    [Serializable]
    public class ParkingStructure
    {
        [SerializeField]
        private ParkingStructureCategory _category = ParkingStructureCategory.Fixed;
        public ParkingStructureCategory Category
        {
            get
            {
                return _category;
            }
        }

        [SerializeField]
        private string _type = "";
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
        private int _remainingSpots = 0;
        public int RemainingSpots
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
        public Vector3 Position
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
        public Vector3 Rotation
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
        public Vector3 Scale
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
        public Vector3 StandbyPosition
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
        public Vector3 LandingQueueHead
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
        public Vector3 LandingQueueDirection
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
        public List<Vector3> ParkingSpots
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

        public Dictionary<Vector3, GameObject> Parked { get; set; } = new Dictionary<Vector3, GameObject>();
        public Dictionary<GameObject, Vector3> VehicleAt { get; set; } = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> Reserved { get; set; } = new Dictionary<GameObject, Vector3>();

        public ParkingStructure()
        {

        }

        public ParkingStructure(ParkingStructure pS)
        {
            _category = pS.Category;
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

        public void Reserve(GameObject vehicle)
        {
            Vector3 spotToReserve = new Vector3();
            bool success = false;
            foreach (Vector3 p in ParkingSpots)
            {
                if (!Parked.ContainsKey(p) && !Reserved.Values.Contains(p))
                {
                    spotToReserve = p;
                    break;
                }
            }
            if (!Reserved.ContainsKey(vehicle))
            {
                Reserved.Add(vehicle, spotToReserve);
                RemainingSpots--;
                Debug.Log(Type + ": " + vehicle.name + " reserved " + spotToReserve.ToString());
                success = true;
            }
            if (!success) Debug.Log(Type + ": Reservation failed");

        }

        public Vector3 Unreserve(GameObject vehicle)
        {
            Vector3 reservedSpot = new Vector3();
            bool success = false;
            if (Reserved.ContainsKey(vehicle))
            {
                reservedSpot = Reserved[vehicle];
                Reserved.Remove(vehicle);
                RemainingSpots++;
                Debug.Log(Type + ": " + vehicle.name + " unreserved " + reservedSpot.ToString());
                success = true;
            }
            if (!success) Debug.Log(Type + ": Reservation failed");
            return reservedSpot;
        }
        public bool ParkAt(Vector3 spot, GameObject vehicle)
        {
            if (Parked.ContainsKey(spot)) return false;
            else
            {
                Parked.Add(spot, vehicle);
                VehicleAt.Add(vehicle, spot);
                RemainingSpots--;
                return true;
            }
        }
        public bool Unpark(GameObject vehicle)
        {
            // if true, there is no such vehicle parked in this structure
            if (!VehicleAt.ContainsKey(vehicle)) return false;
            else
            {
                Vector3 spot = VehicleAt[vehicle];
                Parked.Remove(spot);
                VehicleAt.Remove(vehicle);
                RemainingSpots++;
                return true;
            }
        }

        public Vector3 GetEmptySpot()
        {
            Vector3 emptySpot = new Vector3();
            foreach (Vector3 v in ParkingSpots)
            {
                if (!Parked.ContainsKey(v))
                {
                    emptySpot = v;
                    break;
                }
            }
            return emptySpot;
        }

        // Translates to the global coordinate
        public Vector3 TranslateParkingSpot(Vector3 parkingSpot)
        {
            return (Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z) * parkingSpot + Position);
        }

        public List<Vector3> GetParkingGuide(Vector3 spot, string mode, string type)
        {
            // mode == {parking, unparking}
            // Temporary function

            List<Vector3> guides = new List<Vector3>();
            if (type.Equals("Simple_4Way_Stack"))
            {
                Vector3 direction = new Vector3(spot.x, 0.0f, spot.z).normalized;
                direction = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z) * direction;
                guides.Add(spot);
                Vector3 current_spot = spot + direction * 20.0f;
                guides.Add(current_spot);
                current_spot.y = StandbyPosition.y;
                guides.Add(current_spot);
                guides.Add(StandbyPosition);
            }
            else if (type.Equals("generic_rectangular_lot"))
            {
                guides.Add(spot);
                guides.Add(StandbyPosition);
            }

            if (mode == "parking") guides.Reverse();

            return guides;
        }



    }

}
