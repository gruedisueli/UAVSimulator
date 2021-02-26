using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class ParkingStructure
    {
        public string type;
        public int remainingSpots;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 standbyPosition;
        public Vector3 landingQueueHead;
        public Vector3 landingQueueDirection;
        public List<Vector3> parkingSpots;

        public Dictionary<Vector3, GameObject> parked = new Dictionary<Vector3, GameObject>();
        public Dictionary<GameObject, Vector3> vehicleAt = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> reserved = new Dictionary<GameObject, Vector3>();

        public ParkingStructure()
        {

        }

        public ParkingStructure(ParkingStructure pS)
        {
            type = pS.type;
            remainingSpots = pS.remainingSpots;
            position = pS.position;
            rotation = pS.rotation;
            scale = pS.scale;
            standbyPosition = pS.standbyPosition;
            landingQueueHead = pS.landingQueueHead;
            landingQueueDirection = pS.landingQueueDirection;
            parkingSpots = new List<Vector3>(pS.parkingSpots);
            remainingSpots = parkingSpots.Count;
        }

        /// <summary>
        /// Applies a parking grid to current configuration of structure, using predefined rules. REMOVES any existing parking spots and rebuilds list.
        /// </summary>
        public void ApplyParkingGrid()
        {
            //TO-DO: Use real numbers for margins - now 20m
            parkingSpots = new List<Vector3>();
            int parkingMargin = 20;
            for (int i = (int)(-(scale.x / 2)) + parkingMargin; i <= (int)(scale.x / 2) - parkingMargin; i += parkingMargin)
            {
                for (int j = (int)(-(scale.z / 2)) + parkingMargin; j <= (int)(scale.z / 2) - parkingMargin; j += parkingMargin)
                {
                    Vector3 v = new Vector3((float)i, 0.0f, (float)j);
                    parkingSpots.Add(v);
                }
            }
            remainingSpots = parkingSpots.Count;
        }

        public void Reserve(GameObject vehicle)
        {
            Vector3 spotToReserve = new Vector3();
            bool success = false;
            foreach (Vector3 p in parkingSpots)
            {
                if (!parked.ContainsKey(p) && !reserved.Values.Contains(p))
                {
                    spotToReserve = p;
                    break;
                }
            }
            if (!reserved.ContainsKey(vehicle))
            {
                reserved.Add(vehicle, spotToReserve);
                remainingSpots--;
                Debug.Log(type + ": " + vehicle.name + " reserved " + spotToReserve.ToString());
                success = true;
            }
            if (!success) Debug.Log(type + ": Reservation failed");

        }

        public Vector3 Unreserve(GameObject vehicle)
        {
            Vector3 reservedSpot = new Vector3();
            bool success = false;
            if (reserved.ContainsKey(vehicle))
            {
                reservedSpot = reserved[vehicle];
                reserved.Remove(vehicle);
                remainingSpots++;
                Debug.Log(type + ": " + vehicle.name + " unreserved " + reservedSpot.ToString());
                success = true;
            }
            if (!success) Debug.Log(type + ": Reservation failed");
            return reservedSpot;
        }
        public bool ParkAt(Vector3 spot, GameObject vehicle)
        {
            if (parked.ContainsKey(spot)) return false;
            else
            {
                parked.Add(spot, vehicle);
                vehicleAt.Add(vehicle, spot);
                remainingSpots--;
                return true;
            }
        }
        public bool Unpark(GameObject vehicle)
        {
            // if true, there is no such vehicle parked in this structure
            if (!vehicleAt.ContainsKey(vehicle)) return false;
            else
            {
                Vector3 spot = vehicleAt[vehicle];
                parked.Remove(spot);
                vehicleAt.Remove(vehicle);
                remainingSpots++;
                return true;
            }
        }

        public Vector3 GetEmptySpot()
        {
            Vector3 emptySpot = new Vector3();
            foreach (Vector3 v in parkingSpots)
            {
                if (!parked.ContainsKey(v))
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
            return (Quaternion.Euler(rotation.x, rotation.y, rotation.z) * parkingSpot + position);
        }

        public List<Vector3> GetParkingGuide(Vector3 spot, string mode, string type)
        {
            // mode == {parking, unparking}
            // Temporary function

            List<Vector3> guides = new List<Vector3>();
            if (type.Equals("Simple_4Way_Stack"))
            {
                Vector3 direction = new Vector3(spot.x, 0.0f, spot.z).normalized;
                direction = Quaternion.Euler(rotation.x, rotation.y, rotation.z) * direction;
                guides.Add(spot);
                Vector3 current_spot = spot + direction * 20.0f;
                guides.Add(current_spot);
                current_spot.y = standbyPosition.y;
                guides.Add(current_spot);
                guides.Add(standbyPosition);
            }
            else if (type.Equals("generic_rectangular_lot"))
            {
                guides.Add(spot);
                guides.Add(standbyPosition);
            }

            if (mode == "parking") guides.Reverse();

            return guides;
        }



    }

}
