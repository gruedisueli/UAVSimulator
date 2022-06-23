using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;

public class LandingPoint : MonoBehaviour
{

    
    // Communication related variables
    public GameObject currentTarget;  // Current Communication Target
    public string signal;             // Emitting signal

    // State Machine related variables
    public string state;
    private List<Queue<GameObject>> arrivalQueue;
    private Queue<GameObject> departureQueue;
    private GameObject vehicleOnThisPoint;

    private string landingName;

    public Vector3 baseLocation;
    public List<Vector3> landingStandByPoints;
    public Dictionary<Vector3, Vector3> landingQueueDirection = new Dictionary<Vector3, Vector3>();

    public List<Vector3> departureStandByPoints;

    private Dictionary<Vector3, Queue<Vector3>> landingGuides;

    //To-do: depart following departure guide 
    //       also need to change behavior designer
    private Dictionary<Vector3, Queue<Vector3>> departureGuides;

    private SignalSystem signalSystem;

    // Start is called before the first frame update
    void Start()
    {
        landingName = null;
        baseLocation = new Vector3();
        currentTarget = null;
        signal = null;
        state = "idle";
        arrivalQueue = new List<Queue<GameObject>>();
        departureQueue = new Queue<GameObject>();
        landingStandByPoints = new List<Vector3>();
        departureStandByPoints = new List<Vector3>();
        landingGuides = new Dictionary<Vector3, Queue<Vector3>>();
        departureGuides = new Dictionary<Vector3, Queue<Vector3>>();
        signalSystem = GameObject.Find("SignalSystem").GetComponent<SignalSystem>();
        vehicleOnThisPoint = null;
        baseLocation = gameObject.GetComponent<MeshRenderer>().bounds.center;
    }

    

    // Update is called once per frame

    void Update()
    {
        

        if (signalSystem.hasSignaltoReceive(gameObject))
        {
            VehicleControlSignal vcs = signalSystem.UnregisterSignal(gameObject);

            if (state == "idle")
            {
                if ( vcs.signal == "call" ) // if a vehicle on this point got a call
                {
                    // if the state is idle, the vehicle can take-off immediately without queueing
                    
                    state = "departing";
                    vehicleOnThisPoint = vcs.from;
                    signalSystem.RegisterSignal(vcs.from, new VehicleControlSignal(gameObject, "takeoffgranted"));
                    // destination list is already passed to the vehicle from the central system
                }
                else if ( !IsArrivalQueueEmpty() ) // if there are vehicles waiting to land
                {
                    var queuetoDequeue = ArrivalDequeue();
                    signalSystem.RegisterSignal(queuetoDequeue.Dequeue(), new VehicleControlSignal(gameObject, "landinggranted"));
                    foreach(GameObject g in queuetoDequeue)
                    {
                        signalSystem.RegisterSignal(g, new VehicleControlSignal(gameObject, "moveforward"));
                    }
                    state = "landing";
                }
            }
            else if (state == "landing")
            {
                if ( vcs.signal == "arrived" )
                {
                    state = "arrived";
                    vehicleOnThisPoint = vcs.from;
                    if(!vcs.from.GetComponent<DroneBase>().ToPark || (vcs.from.GetComponent<DroneBase>().ToPark && vcs.from.GetComponent<DroneBase>().DestinationQueue.Count > 0)) departureQueue.Enqueue(vcs.from);
                    signalSystem.RegisterSignal(gameObject, new VehicleControlSignal(vcs.from, "dummy"));
                    // ways to get removed from departureQueue
                    // 1. departure
                    // 2. parking
                }
            }
            else if (state == "arrived")
            {

                // To-Do: add to "move to parking spot" - corresponding changes for 
                // wait for a random period
                if (!vcs.from.GetComponent<DroneBase>().ToPark || (vcs.from.GetComponent<DroneBase>().ToPark && vcs.from.GetComponent<DroneBase>().DestinationQueue.Count > 0 ))
                {
                    state = "departing";
                    signalSystem.RegisterSignal(DepartureDequeue(), new VehicleControlSignal(gameObject, "takeoffgranted"));
                }
                else if (vcs.from.GetComponent<DroneBase>().ToPark && vcs.from.GetComponent<DroneBase>().DestinationQueue.Count == 0 )
                {
                    GameObject vehicle = vcs.from;
                    vehicle.GetComponent<DroneBase>().State = "standby";
                    //vehicle.GetComponent<Vehicle>().currentPoint.GetComponent<Parking>().Park( vehicle );
                    vehicle.GetComponent<DroneBase>().ToPark = false;
                    state = "idle";
                }
            }
            else if (state == "departing")
            {
                if ( vcs.signal == "moving" )
                {
                    state = "idle";
                    vehicleOnThisPoint = null;
                }
            }
        }
        else
        {
            if (state == "idle" && !IsArrivalQueueEmpty())
            {
                var queuetoDequeue = ArrivalDequeue();
                signalSystem.RegisterSignal(queuetoDequeue.Dequeue(), new VehicleControlSignal(gameObject, "landinggranted"));
                foreach (GameObject g in queuetoDequeue)
                {
                    signalSystem.RegisterSignal(g, new VehicleControlSignal(gameObject, "moveforward"));
                }
                state = "landing";
            }
        }
    }


    #region Private Methods
    private bool IsArrivalQueueEmpty ()
    {
        if (arrivalQueue.Count == 0) return true;
        else
        {
            foreach (Queue<GameObject> this_queue in arrivalQueue)
            {
                if (this_queue.Count > 0) return false;
            }
        }
        return true;
    }

    private Queue<GameObject> ArrivalDequeue ()
    {
        List<Queue<GameObject>> unempty_queues = new List<Queue<GameObject>>();
        foreach (Queue<GameObject> this_queue in arrivalQueue)
        {
            if (this_queue.Count > 0) unempty_queues.Add(this_queue);
        }

        // randomly select which one to dequeue
        int selected_queue = (int) Mathf.RoundToInt(Random.Range(0, (float)unempty_queues.Count - 0.6f));
        if (selected_queue >= unempty_queues.Count) selected_queue = unempty_queues.Count - 1;
        return unempty_queues[selected_queue];
    }

    private GameObject DepartureDequeue()
    {
        return departureQueue.Dequeue();
    }

    private Vector3 GetDeparturePoint()
    {
        // TO-DO: Finding a departure point that is closest to the next destiation of the UAV
        // UAV's destination has been already updated bt this point, so don't worry
        // Now, it just returnds the first Departure Point. If departure point is not defined, use a point that is directly above it.
        if (departureStandByPoints.Count > 0) return departureStandByPoints[0];
        else return new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 50, gameObject.transform.position.z);
    }
    #endregion
    #region Public Methods
    public Vector3 GetClosestLandingStandby(Vector3 currentPoint)
    {
        Vector3 closest = new Vector3();
        float min_dist = Mathf.Infinity;
        if (landingStandByPoints.Count == 0) return (new Vector3(baseLocation.x, currentPoint.y, baseLocation.z));
        foreach(Vector3 p in landingStandByPoints)
        {
            if (Vector3.Distance(currentPoint, p + baseLocation) < min_dist )
            {
                min_dist = Vector3.Distance(currentPoint, p + baseLocation);
                closest = p + baseLocation;
            }
        }
        return closest;
    }

    public void Register(string name, Vector3 basePoint)
    {
        this.landingName = name;
        this.baseLocation = basePoint;
    }
    public void RegisterArrivalInfo(Vector3 point, Queue<Vector3> guides)
    {
        // if there is no "key -> point", then no landing guide information is provided
        // in that case, use a straight line from the point to the 
        landingStandByPoints.Add(point);
        if (guides != null) landingGuides.Add(point, guides);
    }

    public void RegisterDepartureInfo(Vector3 point, Queue<Vector3> guides)
    {
        // if there is no "key -> point", then no landing guide information is provided
        // in that case, use a straight line from the point to the 
        departureStandByPoints.Add(point);
        if (guides != null) departureGuides.Add(point, guides);
    }

    public int AddToArrivalQueue(Vector3 standby_point, GameObject vehicle)
    {
        for (int i = 0; i < landingStandByPoints.Count; i++)
        {
            if (standby_point.Equals(landingStandByPoints[i]))
            {
                arrivalQueue[i].Enqueue(vehicle);
                return (arrivalQueue[i].Count - 1);
            }
        }
        if (landingStandByPoints.Count == 0)
        {
            landingStandByPoints.Add(new Vector3(0, vehicle.transform.position.y, 0));
            arrivalQueue.Add(new Queue<GameObject>());
        }
        arrivalQueue[0].Enqueue(vehicle);
        return (arrivalQueue[0].Count - 1);
    }

    public Vector3 GetLandingStandbyQueueDirection ( Vector3 standbyQueuePoint, Vector3 currentLocation )
    {
        if (landingQueueDirection.ContainsKey(standbyQueuePoint))
            return landingQueueDirection[standbyQueuePoint];
        else
        {
            landingQueueDirection.Add(standbyQueuePoint, (currentLocation - standbyQueuePoint).normalized + new Vector3(0,0.25f, 0));
            return landingQueueDirection[standbyQueuePoint];
        }
    }
    public Queue<Vector3> GetLandingGuide(Vector3 landingStandbyPoint)
    {
        if (landingGuides.ContainsKey(landingStandbyPoint)) return landingGuides[landingStandbyPoint];
        else
        {
            Queue<Vector3> result = new Queue<Vector3>();
            result.Enqueue(new Vector3(0,0,0));
            return result;
        }

    }
    #endregion




}
