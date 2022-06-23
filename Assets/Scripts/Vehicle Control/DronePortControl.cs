using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;

/// <summary>
/// This is like a control tower for a single drone port.
/// </summary>
public class DronePortControl : TrafficControl
{
    
    public DronePortBase dronePortInfo;

    /// <summary>
    /// Resets to simulation start conditions
    /// </summary>
    public void ResetSimulation()
    {
        ResetTrafficControlSim();
    }

    /// <summary>
    /// Assigns a landing corridor (waypoints) to a specific drone registered to the control tower.
    /// </summary>
    protected override void AssignLandingCorridor()
    {
        List<Vector3> landingGuide = dronePortInfo.GetLandingGuide("landing");
        List<Vector3> translatedLandingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedLandingGuide.Add(dronePortInfo.TranslateLandingGuidePosition(v));
        vehicleState.WayPointsQueue = toQueue(translatedLandingGuide);
    }

    /// <summary>
    /// Assigns a take off corridor (waypoints) to a specific drone registered to the control tower.
    /// </summary>
    protected override void AssignTakeOffCorridor()
    {
        List<Vector3> landingGuide = dronePortInfo.GetLandingGuide("takeoff");
        List<Vector3> translatedLandingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedLandingGuide.Add(dronePortInfo.TranslateLandingGuidePosition(v));
        vehicleState.WayPointsQueue = toQueue(translatedLandingGuide);
    }
}
