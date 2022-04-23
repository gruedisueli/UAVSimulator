using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataStructure;
using Assets.Scripts.SimulatorCore;

/// <summary>
/// Base class for control towers such as those on drone ports and parking structures.
/// </summary>
public abstract class TrafficControl : MonoBehaviour
{
    protected bool busy { get; set; } // = {idle, busy}
    protected Queue<GameObject> queue { get; set; }//@Eunu comment. how do we manage take-off and landing queues? Aren't they different? Or does everything just get put into one queue since the tower can only do one thing at a time?
    public int queueLength
    {
        get
        {
            return queue.Count;
        }
    }
    protected GameObject currentVehicle { get; set; }//vehicle that is currently executing takeoff/landing? @Eunu comment
    protected DroneBase vehicleState { get; set; }//@Eunu comment
    private SimulationAnalyzer _simulationAnalyzer;
    protected DroneInstantiator _droneInstantiator;
    protected VehicleControlSystem _vcs;

    private void Awake()
    {
        _droneInstantiator = FindObjectOfType<DroneInstantiator>(true);
        if (_droneInstantiator == null)
        {
            Debug.Log("Could not locate drone instantiator component");

        }
        queue = new Queue<GameObject>();
        _simulationAnalyzer = FindObjectOfType<SimulationAnalyzer>(true);
        if (_simulationAnalyzer == null)
        {
            Debug.Log("Could not locate simulation analyzer component");
        }

        _vcs = FindObjectOfType<VehicleControlSystem>(true);
        if (_vcs == null)
        {
            Debug.Log("Could not locate vehicle control system component");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_vcs.playing) return;
        if (queueLength > 3)//this is congested
        {
            if (this is ParkingControl && !_simulationAnalyzer.congestedParkingStructures.Contains(this.gameObject))
            {
                _simulationAnalyzer.congestedParkingStructures.Add(this.gameObject);
            }
            else if (this is DronePortControl && !_simulationAnalyzer.congestedDronePorts.Contains(this.gameObject))
            {
                _simulationAnalyzer.congestedDronePorts.Add(this.gameObject);
            }
        }
        else//not congested
        {
            if (this is ParkingControl && _simulationAnalyzer.congestedParkingStructures.Contains(this.gameObject))
            {
                _simulationAnalyzer.congestedParkingStructures.Remove(this.gameObject);
            }
            else if (this is DronePortControl && _simulationAnalyzer.congestedDronePorts.Contains(this.gameObject))
            {
                _simulationAnalyzer.congestedDronePorts.Remove(this.gameObject);
            }
        }

        if ( !busy && queue.Count > 0 )//get next vehicle in queue
        {
            currentVehicle = queue.Dequeue();
            vehicleState = currentVehicle.GetComponent<DroneBase>();
            busy = true;
        }
        else if ( vehicleState != null )
        {
            if (vehicleState.State == "pending")//if vehicle is waiting for landing to be granted, grant it.
            {
                AssignLandingCorridor();
                GrantLanding();
            }
            else if (vehicleState.State == "idle" )//if vehicle is sitting idle on pad, allow takeoff
            {
                AssignTakeOffCorridor();
                GrantTakeOff();
            }
        }
    }
    /// <summary>
    /// Resets to default conditions.
    /// </summary>
    protected void ResetTrafficControlSim()
    {
        busy = false;
        queue.Clear();
        currentVehicle = null;
        vehicleState = null;
    }


    /// <summary>
    /// Assigns a landing corridor to the specific vehicle we are going to send commands to.
    /// </summary>
    protected virtual void AssignLandingCorridor()
    {
        vehicleState.WayPointsQueue = new Queue<Vector3>();
        vehicleState.WayPointsQueue.Enqueue(gameObject.transform.position);
    }

    /// <summary>
    /// To be implemented in the derived classes
    /// </summary>
    protected virtual void AssignTakeOffCorridor()
    {
        
    }

    /// <summary>
    /// @Eunu comment
    /// </summary>
    public void FreeUp()
    {
        currentVehicle = null;
        vehicleState = null;
        busy = false;
    }

    /// <summary>
    /// Adds a drone to the queue.
    /// </summary>
    public void RegisterInQueue(GameObject drone)
    {
        queue.Enqueue(drone);
    }

    /// <summary>
    /// Grants landing to current vehicle we are issuing commands to.
    /// </summary>
    private void GrantLanding()
    {
        currentVehicle.SendMessage("LandGranted");
    }

    /// <summary>
    /// Grants takeoff to current vehicle we are issuing commands to.
    /// </summary>
    private void GrantTakeOff()
    {
        currentVehicle.SendMessage("TakeoffGranted");
    }

    /// <summary>
    /// @Eunu comment
    /// </summary>
    protected Queue<Vector3> toQueue(List<Vector3> points)
    {
        Queue<Vector3> result = new Queue<Vector3>();
        foreach (Vector3 p in points)
        {
            result.Enqueue(p);
        }
        return result;
    }

    /// <summary>
    /// Determine if a vehicle is in the queue.
    /// </summary>
    public bool QueueContains(GameObject v)
    {
        if (queue.Contains(v)) return true;
        else return false;
    }

}
