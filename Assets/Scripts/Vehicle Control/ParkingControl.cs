using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;

/// <summary>
/// This is like a control tower for a single parking structure.
/// </summary>
public class ParkingControl : TrafficControl
{
    public ParkingStructureBase parkingInfo;
    public int RemainingSpots
    {
        get
        {
            return parkingInfo.ParkingSpots.Count - Reserved.Count - Parked.Count;
        }
    }
    public Dictionary<Vector3, GameObject> Parked { get; set; } = new Dictionary<Vector3, GameObject>();
    public Dictionary<GameObject, Vector3> VehicleAt { get; set; } = new Dictionary<GameObject, Vector3>();
    public Dictionary<GameObject, Vector3> Reserved { get; set; } = new Dictionary<GameObject, Vector3>();

    /// <summary>
    /// Resets to simulation start conditions
    /// </summary>
    public void ResetSimulation()
    {
        Parked.Clear();
        VehicleAt.Clear();
        Reserved.Clear();
        ResetTrafficControlSim();
    }

    /// <summary>
    /// Assigns a landing corridor (waypoints) to a specific drone registered to this parking structure.
    /// </summary>
    protected override void AssignLandingCorridor()
    {
        Vector3 parkingSpot = GetEmptySpot();
        List<Vector3> landingGuide = GetParkingGuide(parkingSpot, "parking", parkingInfo.Type);
        List<Vector3> translatedParkingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedParkingGuide.Add(TranslateParkingSpot(v));
        vehicleState.WayPointsQueue = toQueue(translatedParkingGuide);
        ParkAt(parkingSpot, currentVehicle);
    }

    /// <summary>
    /// Assigns a takeoff corridor (waypoints) to a specific drone registered to this parking structure.
    /// </summary>
    protected override void AssignTakeOffCorridor()
    {
        Vector3 parkingSpot = VehicleAt[currentVehicle];
        List<Vector3> takeoffGuide = GetParkingGuide(parkingSpot, "unparking", parkingInfo.Type);
        List<Vector3> translatedParkingGuide = new List<Vector3>();
        foreach (Vector3 v in takeoffGuide) translatedParkingGuide.Add(TranslateParkingSpot(v));
        vehicleState.WayPointsQueue = toQueue(translatedParkingGuide);
        Unpark(currentVehicle);
    }

    /// <summary>
    /// Calls a specific vehicle in the parking structure to enter the queue for departure.
    /// </summary>
    public void CallVehecleInParkingStructure(GameObject vehicle)
    {
        queue.Enqueue(vehicle);
    }

    public void Reserve(GameObject vehicle)
    {
        Vector3 spotToReserve = new Vector3();
        bool success = false;
        foreach (Vector3 p in parkingInfo.ParkingSpots)
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
            if (Reserved.ContainsKey(vehicle))
            {
                Debug.Log(parkingInfo.Type + "(" + parkingInfo.Position.ToString() + "): " + vehicle.name + " reserved " + spotToReserve.ToString());
                success = true;
            }
            else
            {
                success = false;
            }
        }
        if (!success) Debug.Log(parkingInfo.Type + ": Reservation failed");

    }

    public Vector3 Unreserve(GameObject vehicle)
    {
        Vector3 reservedSpot = new Vector3();
        bool success = false;
        if (Reserved.ContainsKey(vehicle))
        {
            reservedSpot = Reserved[vehicle];
            Reserved.Remove(vehicle);
            Debug.Log(parkingInfo.Type + ": " + vehicle.name + " unreserved " + reservedSpot.ToString());
            success = true;
        }
        if (!success) Debug.Log(parkingInfo.Type + ": Reservation failed");
        return reservedSpot;
    }
    public bool ParkAt(Vector3 spot, GameObject vehicle)
    {
        if (Parked.ContainsKey(spot)) return false;
        else
        {
            Parked.Add(spot, vehicle);
            VehicleAt.Add(vehicle, spot);
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
            return true;
        }
    }

    public Vector3 GetEmptySpot()
    {
        Vector3 emptySpot = new Vector3();
        foreach (Vector3 v in parkingInfo.ParkingSpots)
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
        return (Quaternion.Euler(parkingInfo.Rotation.x, parkingInfo.Rotation.y, parkingInfo.Rotation.z) * parkingSpot + parkingInfo.Position);
    }

    public List<Vector3> GetParkingGuide(Vector3 spot, string mode, string type)
    {
        // mode == {parking, unparking}
        // Temporary function

        List<Vector3> guides = new List<Vector3>();
        if (type.Equals("Simple_4Way_Stack"))
        {
            Vector3 direction = new Vector3(spot.x, 0.0f, spot.z).normalized;
            direction = Quaternion.Euler(parkingInfo.Rotation.x, parkingInfo.Rotation.y, parkingInfo.Rotation.z) * direction;
            guides.Add(spot);
            Vector3 current_spot = spot + direction * 20.0f;
            guides.Add(current_spot);
            current_spot.y = parkingInfo.StandbyPosition.y;
            guides.Add(current_spot);
            guides.Add(parkingInfo.StandbyPosition);
        }
        else if (type.Equals("Rect"))
        {
            guides.Add(spot);
            guides.Add(parkingInfo.StandbyPosition);
        }

        else if (type.Equals("Circular_Stack_LowAltitude"))
        {
            guides.Add(spot);
            guides.Add(new Vector3(0, spot.y, 0));
            guides.Add(parkingInfo.StandbyPosition);
        }
        if (mode == "parking") guides.Reverse();

        return guides;
    }

    private void OnDestroy()
    {
        //destroy any vehicles inside this structure
        foreach (var v in Parked.Values)
        {
            _droneInstantiator.UnregisterDrone(v);
            v.Destroy();
        }
    }
}
