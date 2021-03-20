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
        public abstract int RemainingSpots { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Vector3 Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 StandbyPosition { get; set; }
        public abstract Vector3 LandingQueueHead { get; set; }
        public abstract Vector3 LandingQueueDirection { get; set; }
        public abstract List<Vector3> ParkingSpots { get; set; }

        public abstract ParkingStructureBase GetCopy();

        public abstract Dictionary<Vector3, GameObject> Parked { get; set; }
        public abstract Dictionary<GameObject, Vector3> VehicleAt { get; set; }
        public abstract Dictionary<GameObject, Vector3> Reserved { get; set; }

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
