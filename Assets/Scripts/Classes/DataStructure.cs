using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DataStructure
{

    [Serializable]
    class VehicleSpec
    {
        public string type;
        public int capacity;
        public float maxSpeed;
        public float landingSpeed;
        public float takeoffSpeed;
        public float yawSpeed;
        public float range;
        public float size;
        public List<float> emission;
        public List<float> noise;
    }
    
    
    #region Environment
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

        public Dictionary<Vector3, GameObject> parked;
        public Dictionary<GameObject, Vector3> vehicleAt;

        public Dictionary<GameObject, Vector3> reserved;

        public void Reserve(GameObject vehicle)
        {
            Vector3 spotToReserve = new Vector3();
            bool success = false;
            foreach(Vector3 p in parkingSpots)
            {
                if (!parked.ContainsKey(p) && !reserved.Values.Contains(p) )
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
        public Vector3 TranslateParkingSpot ( Vector3 parkingSpot )
        {
            return (Quaternion.Euler(rotation.x, rotation.y, rotation.z) * parkingSpot + position);
        }

        public List<Vector3> GetParkingGuide (Vector3 spot, string mode, string type)
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

    [Serializable]
    public class DronePort
    {
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 standbyPosition;
        public Vector3 landingQueueHead;
        public Vector3 landingQueueDirection;
        public Vector3 landingPoint;
        public float maximumVehicleSize;
        public bool isMountable;
        public bool isOnTheGround;
        public bool isScalable;

        // Translates to the global coordinate
        public Vector3 TranslateLandingGuidePosition(Vector3 parkingSpot)
        {
            return (Quaternion.Euler(rotation.x, rotation.y, rotation.z) * parkingSpot + position);
        }

        public List<Vector3> GetLandingGuide(string mode)
        {
            // mode == {parking, unparking}
            // Temporary function

            List<Vector3> guides = new List<Vector3>();

            guides.Add(landingQueueHead);
            guides.Add(standbyPosition);
            guides.Add(landingPoint);
            
            /*
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
            }*/


            if (mode == "takeoff")
            {
                guides.Reverse();
            }

            return guides;
        }
    }

    [Serializable]
    class RestrictionZone
    {
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public List<float> bottoms;
        public List<float> radius;
        public float height;
    }

    [Serializable]
    class Environment
    {
        public List<DronePort> dronePorts;
        public List<ParkingStructure> parkingStructures;
        public List<RestrictionZone> restrictionZones;
    }
    #endregion

    [Serializable]
    #region Simulation Parameters
    public class SimulationParam
    {
        public float maxSpeed;
        public float verticalSeparation;
        public float horizontalSeparation;
        public float maxVehicleCount;
        public float takeoffSpeed;
        public float landingSpeed;
        public float maxYawSpeed;
    }
    #endregion

    [Serializable]
    #region Simulation Info
    class SimulationStat
    {
        public float throughput;
        public float affectedArea;
        public float emission;
        public List<GameObject> vehicles;
    }
    #endregion

    public class Path
    {
        public GameObject origin;
        public GameObject destination;
        public float elevation;
        public List<Vector3> wayPoints;

        public bool Equals(Path p1, Path p2)
        {
            if (p1.origin.transform.position.Equals(p2.origin.transform.position) && p1.destination.transform.position.Equals(p2.destination.transform.position) && Mathf.Abs(p1.elevation - p2.elevation) < 0.001) return true;
            else return false;
        }
    }
}
