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
    /// Assigns a landing corridor (waypoints) to a specific drone registered to the control tower.
    /// </summary>
    protected override void AssignLandingCorridor()
    {
        List<Vector3> landingGuide = dronePortInfo.GetLandingGuide("landing");
        List<Vector3> translatedLandingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedLandingGuide.Add(dronePortInfo.TranslateLandingGuidePosition(v));
        vehicleState.wayPointsQueue = toQueue(translatedLandingGuide);
    }

    /// <summary>
    /// Assigns a take off corridor (waypoints) to a specific drone registered to the control tower.
    /// </summary>
    protected override void AssignTakeOffCorridor()
    {
        List<Vector3> landingGuide = dronePortInfo.GetLandingGuide("takeoff");
        List<Vector3> translatedLandingGuide = new List<Vector3>();
        foreach (Vector3 v in landingGuide) translatedLandingGuide.Add(dronePortInfo.TranslateLandingGuidePosition(v));
        vehicleState.wayPointsQueue = toQueue(translatedLandingGuide);
    }
    /*
    void Start()
    {
        state = "idle";
        queue = new Queue<GameObject>();
        watch = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if ( state == "idle" )
        { 
            if ( queue.Count > 0 )
            {
                currentVehicle = queue.Dequeue();
                currentVehicleState = currentVehicle.GetComponent<Vehicle>();

                if ( queue.Count > 0 )
                {
                    List<GameObject> queueList = new List<GameObject> (queue.ToArray());
                    foreach(GameObject v in queueList)
                    {
                        Vehicle vehicle = v.GetComponent<Vehicle>();
                        int place = placeInQueue(v);
                        vehicle.currentTargetPosition = (gameObject.transform.position + dronePortInfo.LandingQueueHead + dronePortInfo.LandingQueueDirection * (float)place * landingQueueSeparation);
                        vehicle.moveForward = true;
                    }
                }
                if (currentVehicleState.currentPoint != gameObject)
                {
                    return;
                }
                state = "busy";
            }
        }
        else
        {
            if ( currentVehicleState.state == "landing_requested" )
            {
                //Vector3 currentVehicleParkingSpot = parkingInfo.vehicleAt[currentVehicle];  // Get the parking location of the current vehicle
                List<Vector3> landingGuide = dronePortInfo.GetLandingGuide("landing");
                List<Vector3> translatedLandingGuide = new List<Vector3>();
                foreach (Vector3 v in landingGuide) translatedLandingGuide.Add(dronePortInfo.TranslateLandingGuidePosition(v));
                currentVehicleState.assignedLandingGuide = toQueue(translatedLandingGuide);
                currentVehicleState.currentTargetPosition = currentVehicleState.assignedLandingGuide.Dequeue();
                currentVehicleState.state = "landing_granted";
            }
            else if ( currentVehicleState.state == "arrived" )
            {
                watch = 0.0f;
                waitTime = Random.Range(2.0f, 5.0f);
                currentVehicleState.state = "boarding";
            }
            else if (currentVehicleState.state == "boarding")
            {
                watch += Time.deltaTime;
                if ( watch > waitTime )
                {
                    currentVehicleState.state = "takeoff_granted";
                }
            }
            else if (currentVehicleState.state == "move_ready" || currentVehicleState.state == "moving")
            {
                currentVehicle = null;
                state = "idle";
            }

        }
    }
    private Queue<Vector3> toQueue(List<Vector3> points)
    {
        Queue<Vector3> result = new Queue<Vector3>();
        foreach (Vector3 p in points)
        {
            result.Enqueue(p);
        }
        return result;
    }

    public int placeInQueue (GameObject v)
    {
        GameObject[] vehiclesInQueue = queue.ToArray();
        for (int i = 0; i < vehiclesInQueue.Length; i++)
        {
            if (vehiclesInQueue[i].Equals(v)) return i;
        }
        return -1;
    }
    public Vector3 GetStandbyPosition (GameObject v)
    {
        int place = placeInQueue(v);
        if (place < 0)
        {
            if ( !currentVehicle.Equals(v) ) queue.Enqueue(v);
            return new Vector3();
        }
        else
        {
            return (gameObject.transform.position + dronePortInfo.LandingQueueHead + dronePortInfo.LandingQueueDirection * (float)place * landingQueueSeparation);
        }
    }
    */
}
