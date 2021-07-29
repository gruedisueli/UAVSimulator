using System.Collections;
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

    /// <summary>
    /// Assigns a landing corridor (waypoints) to a specific drone registered to this parking structure.
    /// </summary>
    protected override void AssignLandingCorridor()
    {
        Vector3 parkingSpot = parkingInfo.GetEmptySpot();
        List<Vector3> landingGuide = parkingInfo.GetParkingGuide(parkingSpot, "parking", parkingInfo.Type);
        List<Vector3> translatedParkingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedParkingGuide.Add(parkingInfo.TranslateParkingSpot(v));
        vehicleState.wayPointsQueue = toQueue(translatedParkingGuide);
        parkingInfo.ParkAt(parkingSpot, currentVehicle);
    }

    /// <summary>
    /// Assigns a takeoff corridor (waypoints) to a specific drone registered to this parking structure.
    /// </summary>
    protected override void AssignTakeOffCorridor()
    {
        Vector3 parkingSpot = parkingInfo.VehicleAt[currentVehicle];
        List<Vector3> takeoffGuide = parkingInfo.GetParkingGuide(parkingSpot, "unparking", parkingInfo.Type);
        List<Vector3> translatedParkingGuide = new List<Vector3>();
        foreach (Vector3 v in takeoffGuide) translatedParkingGuide.Add(parkingInfo.TranslateParkingSpot(v));
        vehicleState.wayPointsQueue = toQueue(translatedParkingGuide);
        parkingInfo.Unpark(currentVehicle);
    }

    /// <summary>
    /// Calls a specific vehicle in the parking structure to enter the queue for departure.
    /// </summary>
    public void CallVehecleInParkingStructure(GameObject vehicle)
    {
        queue.Enqueue(vehicle);
    }
}
