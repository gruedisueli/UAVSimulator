using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataStructure;
using Assets.Scripts.SimulatorCore;

public abstract class TrafficControl : MonoBehaviour
{
    protected bool busy { get; set; } // = {idle, busy}
    protected Queue<GameObject> queue { get; set; }
    public int queueLength
    {
        get
        {
            return queue.Count;
        }
    }
    protected GameObject currentVehicle { get; set; }
    protected DroneBase vehicleState { get; set; }
    private SimulationAnalyzer sa;

    // Start is called before the first frame update
    void Start()
    {
        queue = new Queue<GameObject>();
        sa = GameObject.Find("SimulationCore").GetComponent<SimulationAnalyzer>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!vcs.playing) return;
        if (queueLength > 3)
        {
            if (this is ParkingControl && !sa.congestedParkingStructures.Contains(this.gameObject))
            {
                sa.congestedParkingStructures.Add(this.gameObject);
            }
            else if (this is DronePortControl && !sa.congestedDronePorts.Contains(this.gameObject))
            {
                sa.congestedDronePorts.Add(this.gameObject);
            }
        }
        else
        {
            if (this is ParkingControl && sa.congestedParkingStructures.Contains(this.gameObject))
            {
                sa.congestedParkingStructures.Remove(this.gameObject);
            }
            else if (this is DronePortControl && sa.congestedDronePorts.Contains(this.gameObject))
            {
                sa.congestedDronePorts.Remove(this.gameObject);
            }
        }
        if ( !busy && queue.Count > 0 )
        {
            currentVehicle = queue.Dequeue();
            vehicleState = currentVehicle.GetComponent<DroneBase>();
            busy = true;
        }

        else if ( vehicleState != null )
        {
            if (vehicleState.state == "pending")
            {
                AssignLandingCorridor();
                GrantLanding();
            }
            else if (vehicleState.state == "idle" )
            {
                AssignTakeOffCorridor();
                GrantTakeOff();
            }
        }
    }



    protected virtual void AssignLandingCorridor()
    {
        vehicleState.wayPointsQueue = new Queue<Vector3>();
        vehicleState.wayPointsQueue.Enqueue(gameObject.transform.position);
    }

    protected virtual void AssignTakeOffCorridor()
    {
        // To be implemented in the derived classes
    }


    public void FreeUp()
    {
        currentVehicle = null;
        vehicleState = null;
        busy = false;
    }

    public void RegisterInQueue(GameObject drone)
    {
        queue.Enqueue(drone);
    }

    
    private void GrantLanding()
    {
        currentVehicle.SendMessage("LandGranted");
    }
    private void GrantTakeOff()
    {
        currentVehicle.SendMessage("TakeoffGranted");
    }

    protected Queue<Vector3> toQueue(List<Vector3> points)
    {
        Queue<Vector3> result = new Queue<Vector3>();
        foreach (Vector3 p in points)
        {
            result.Enqueue(p);
        }
        return result;
    }
    public bool QueueContains(GameObject v)
    {
        if (queue.Contains(v)) return true;
        else return false;
    }

}
