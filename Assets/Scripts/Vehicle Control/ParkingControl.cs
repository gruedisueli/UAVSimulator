using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;

public class ParkingControl : MonoBehaviour
{
    public float landingQueueSeparation = 10.0f;
    public ParkingStructureBase parkingInfo;
    public Queue<GameObject> queue = new Queue<GameObject>();
    public string state; // state = {busy, idle}
    public GameObject currentVehicle;
    public Vehicle currentVehicleState;

 

    void Start()
    {
        state = "idle";
    }
    void Update()
    {
        if ( state == "idle" )
        {
            if (queue.Count > 0 )  // When queue is not empty
            {
                currentVehicle = queue.Dequeue(); 
                currentVehicleState = currentVehicle.GetComponent<Vehicle>();
                
                if (queue.Count > 0)
                {
                    List<GameObject> queueList = new List<GameObject>(queue.ToArray());
                    foreach (GameObject v in queueList)
                    {
                        Vehicle vehicle = v.GetComponent<Vehicle>();
                        if (vehicle.state.Equals("landing_requested"))
                        {
                            int place = placeInQueue(v);
                            vehicle.currentTargetPosition = (gameObject.transform.position + parkingInfo.LandingQueueHead + parkingInfo.LandingQueueDirection * (float)place * landingQueueSeparation);
                            vehicle.moveForward = true;
                        }
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
            if (currentVehicleState.state == "takeoff_requested") // If the vehicle is parked
            {
                // TO-DO: Exception handlings
                Vector3 currentVehicleParkingSpot = parkingInfo.VehicleAt[currentVehicle];  // Get the parking location of the current vehicle
                List<Vector3> parkingGuide = parkingInfo.GetParkingGuide(currentVehicleParkingSpot, "unparking", parkingInfo.Type);
                List<Vector3> translatedParkingGuide = new List<Vector3>();
                foreach (Vector3 v in parkingGuide) translatedParkingGuide.Add(parkingInfo.TranslateParkingSpot(v));
                currentVehicleState.assignedLandingGuide = toQueue(translatedParkingGuide);
                currentVehicleState.currentTargetPosition = currentVehicleState.assignedLandingGuide.Dequeue();
                currentVehicleState.state = "takeoff_granted";
                
                parkingInfo.Unpark(currentVehicle);
            }
            else if ( currentVehicleState.state == "move_ready" || currentVehicleState.state == "moving" )
            {
                currentVehicle = null;
                state = "idle";
            }
            else if (currentVehicleState.state == "landing_requested")
            {
                try
                {
                    Vector3 parkingSpotTemp = parkingInfo.Reserved[currentVehicle];
                }
                catch(KeyNotFoundException e)
                {
                    Debug.Log("???");
                }
                Vector3 parkingSpot = parkingInfo.Reserved[currentVehicle];
                List<Vector3> landingGuide = parkingInfo.GetParkingGuide(parkingSpot, "parking", parkingInfo.Type);
                List<Vector3> translatedParkingGuide = new List<Vector3>();
                foreach (Vector3 v in landingGuide) translatedParkingGuide.Add(parkingInfo.TranslateParkingSpot(v));
                currentVehicleState.assignedLandingGuide = toQueue(translatedParkingGuide);
                currentVehicleState.currentTargetPosition = currentVehicleState.assignedLandingGuide.Dequeue();
                currentVehicleState.state = "landing_granted";
            }
            else if ( currentVehicleState.state == "arrived" )
            {
                Vector3 parkingSpot = parkingInfo.Reserved[currentVehicle];
                parkingInfo.ParkAt(parkingSpot, currentVehicle);
                parkingInfo.Unreserve(currentVehicle);
                currentVehicleState.state = "parked";
                currentVehicleState.toPark = false;
                currentVehicleState.isUTM = false;
                TrailRenderer tr = currentVehicle.GetComponent<TrailRenderer>();
                tr.Clear();
                currentVehicle = null;
                state = "idle";
            }
            else if ( currentVehicleState.state == "parked" )
            {
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

    public int placeInQueue(GameObject v)
    {
        GameObject[] vehiclesInQueue = queue.ToArray();
        for (int i = 0; i < vehiclesInQueue.Length; i++)
        {
            if (vehiclesInQueue[i] == v) return i;
        }
        return -1;
    }
    public Vector3 GetStandbyPosition(GameObject v)
    {
        int place = placeInQueue(v);
        if (place < 0)
        {
            if ( !currentVehicle.Equals(v) ) queue.Enqueue(v);
            return new Vector3();
        }
        else
        {
            return (gameObject.transform.position + parkingInfo.LandingQueueHead + parkingInfo.LandingQueueDirection * (float)place * landingQueueSeparation);
        }
    }


}
