using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.DataStructure;

/// <summary>
/// @Eunu is this obsolete?
/// </summary>
public class Vehicle : MonoBehaviour
{
    // constants


    public float arrival_threshold = 5.0f;
    public float approaching_threshold = 200.0f;

    // Vehicle specific information - can be modified in runtime       
    public int id;
    public string type;
    public int capacity;
    public float maxSpeed;
    public float yawSpeed;
    public float takeOffSpeed;
    public float landingSpeed;
    public float range;
    public List<float> emission;
    public List<float> noise;

    // Analysis/control related information
    public GameObject currentPoint;
    public Vector3 currentLocation;
    public Vector3 currentTargetPosition;
    public float currentSpeed;
    public float elevation;
    public string origin;
    public Queue<GameObject> destination;
    public GameObject[] destinationList;
    //Debug
    public List<Vector3> wayPoints;
    public Queue<Vector3> wayPointsQueue;

    public float separation;
    private Time elapsedTime;

    // State machine variable
    // parked, takeoff, readytomove, moving, approached, landing, arrived
    public string state;

    private SignalSystem signalSystem;
    public int placeInQueue;
    private float landingElevation;
    public Queue<Vector3> assignedLandingGuide;
    public bool toPark;
    public bool moveForward;
    public bool isUTM;
    public bool isBackgroundDrone;
    public float waitTimer;
    public float waitTime;

    public event EventHandler<EventArgs> OnDroneTakeOff;
    public event EventHandler<EventArgs> OnDroneParking;
    private EventArgs e;



    VehicleControlSystem vcs;
    TrailRenderer tr;

    #region Constructors

    public void SetVehicleInfo(string type, int capacity, float range, float maxSpeed, float yawspeed, float takeOffSpeed, float landingSpeed, List<float> emission, List<float> noise)
    {
        this.type = type;
        this.capacity = capacity;
        this.maxSpeed = maxSpeed;
        this.yawSpeed = yawspeed;
        this.takeOffSpeed = takeOffSpeed * 2;
        this.landingSpeed = landingSpeed * 2;
        this.emission = emission;
        this.noise = noise;
        this.range = range;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        e = new EventArgs();

        arrival_threshold = 0.5f;
        wayPoints = new List<Vector3>();
        wayPointsQueue = new Queue<Vector3>();
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        tr = gameObject.GetComponent<TrailRenderer>();
        elevation = Mathf.NegativeInfinity;
        state = "parked";
        toPark = false;
        moveForward = false;
        isUTM = false;
        waitTimer = 0.0f;
        waitTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
    }
        /*
        if (!vcs.playing) currentSpeed = 0;
        else currentSpeed = maxSpeed;
        if (vcs.trailVisualization) tr.enabled = true;
        else tr.enabled = false;
        // In case of background drone, just keep moving around
        currentLocation = gameObject.transform.position;
        if (isBackgroundDrone)
        {
            state = "background";
            currentSpeed = maxSpeed * vcs.speedMultiplier;
            if (wayPointsQueue.Count == 0 && Vector3.Distance(currentTargetPosition, transform.position) < arrival_threshold )
            {
                wayPoints = vcs.FindPath(transform.position, vcs.GetRandomPointXZ(transform.position.y), 5);
                foreach (Vector3 p in wayPoints)
                    wayPointsQueue.Enqueue(p);
                currentTargetPosition = wayPointsQueue.Dequeue();
                
            }
            else
            {
                MoveAlong();
            }
        }
        //destinationList = destination.ToArray();

        //if (state == null) return;
        else
        {
            if (currentTargetPosition.x == 0.0 && state != "parked" && state != "takeoff_requested")
            {
                Debug.Log("Weird!!");
            }
            if (state == "parked" && vcs.movingVehicles.Contains(gameObject))
            {
                vcs.movingVehicles.Remove(gameObject);
                OnDroneParking?.Invoke(gameObject, e);

            }
            if (state != "parked" && !vcs.movingVehicles.Contains(gameObject))
            {
                vcs.movingVehicles.Add(gameObject);
                OnDroneTakeOff?.Invoke(gameObject, e);
            }


            if (state == "takeoff_requested")
            {

            }
            else if (state == "takeoff_granted")
            {
                currentSpeed = takeOffSpeed * vcs.speedMultiplier;
                if (elevation < 0.0f)
                {
                    if (destination.Count > 0) elevation = vcs.GetElevation(currentPoint, destination.Peek());
                    else
                    {
                        GameObject reservedParking = vcs.ReserveNearestAvailableParking(gameObject);
                        List<GameObject> parking_list = new List<GameObject>();
                        parking_list.Add(reservedParking);
                        destination = vcs.Route(currentPoint, parking_list);
                        elevation = vcs.GetElevation(currentPoint, destination.Peek());
                        toPark = true;
                    }
                }
                state = "taking_off";
            }
            else if (state == "operation_point_arrived")
            {
                waitTimer += Time.deltaTime;
                if (waitTimer > waitTime)
                {
                    if (destination.Count > 0)
                    {
                        waitTimer = 0.0f;
                        state = "move_ready";
                    }
                    else
                    {
                        GameObject reservedParking = vcs.ReserveNearestAvailableParking(gameObject);
                        destination.Enqueue(reservedParking);
                        elevation = vcs.GetElevation(currentPoint, destination.Peek());
                        toPark = true;
                        state = "move_ready";
                    }
                }
            }
            else if (state == "taking_off")
            {
                TakeOff();
            }
            else if (state == "move_ready")
            {
                GameObject currentDestination;
                int j = 0;
                try
                {
                    currentDestination = destination.Dequeue();
                }
                catch (System.InvalidOperationException e)
                {
                    Debug.Log("Exception " + this.name);
                    Debug.Log("Current Location: " + currentLocation.ToString());
                    Debug.Log("Current Point: " + currentPoint.name.ToString());
                    currentDestination = currentPoint;
                }*/
                /*
                Vector3 currentDestinationVector = currentDestination.transform.position;
                currentDestinationVector.y = elevation;

                wayPoints = vcs.FindPath(gameObject.transform.position, currentDestinationVector, 5);*/
                //if (isUTM) wayPoints.Add(currentDestination.transform.position);
                /*foreach(Corridor c in vcs.network.outEdges[currentPoint])
                {
                    if ( c.destination.Equals(currentDestination) )
                    {
                        wayPointsQueue = new Queue<Vector3>(c.wayPoints.ToArray());
                        break;
                    }
                }*/
                /*
                wayPointsQueue = new Queue<Vector3>();
                foreach (Vector3 v in wayPoints)
                {
                    wayPointsQueue.Enqueue(v);
                }*/
                /*
                currentTargetPosition = wayPointsQueue.Dequeue();
                currentSpeed = maxSpeed * vcs.speedMultiplier;
                currentPoint = currentDestination;
                state = "moving";
            }
            else if (state == "moving")
            {
                MoveAlong();
            }
            else if (state == "approaching")
            {
                Vector3 standbyPosition = new Vector3();
                if (currentPoint.name.Contains("Drone"))
                {
                    DronePortControl dp = currentPoint.GetComponent<DronePortControl>();
                    if (!dp.queue.Contains(gameObject)) dp.queue.Enqueue(gameObject);
                    standbyPosition = dp.GetStandbyPosition(gameObject);
                }
                else
                {
                    ParkingControl p = currentPoint.GetComponent<ParkingControl>();
                    if (p == null)
                    {
                        Debug.Log("approaching - exception");
                    }
                    if (!p.queue.Contains(gameObject))
                    {
                        p.queue.Enqueue(gameObject);
                    }
                    standbyPosition = p.GetStandbyPosition(gameObject);
                }
                state = "registeredInQueue";
                currentTargetPosition = standbyPosition;
            }
            else if (state == "registeredInQueue")
            {
                MoveAlong();
            }
            else if (state == "landing_requested")
            {
                currentSpeed = landingSpeed * vcs.speedMultiplier;
                if (moveForward == true)
                {
                    state = "registeredInQueue";
                }
                // When assigned new standby position
            }
            else if (state == "landing_granted")
            {
                moveForward = false;
                //currentSpeed = landingSpeed * vcs.speedMultiplier;
                Land();
            }



            //UpdateToSystem(this);
            //if ( destination.Count > 0 )  nextDestination = destination.Peek();
        }
        
    }
    void TakeOff()
    {
        if (assignedLandingGuide.Count == 0) currentTargetPosition.y = elevation;
        if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
        {
            if (assignedLandingGuide.Count == 0)
            {
                state = "move_ready";
                return;
            }
            else
            {
                currentTargetPosition = assignedLandingGuide.Dequeue();
                return;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime );
        }
    }

    
    void Land()
    {
        //if (assignedLandingGuide.Count == 0) currentTargetPosition.y = elevation;
        if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
        {
            if (assignedLandingGuide.Count == 0)
            {
                state = "arrived";
                elevation = float.NegativeInfinity;
                return;
            }
            else
            {
                currentTargetPosition = assignedLandingGuide.Dequeue();
                return;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
        }
    }
    void MoveAlong()
    {
        if (!isUTM)
        {
            if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
            {
                if (wayPointsQueue.Count == 0)
                {
                    state = "landing_requested";
                    return;
                }
                else
                {
                    currentTargetPosition = wayPointsQueue.Dequeue();
                    return;
                }
            }
            if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < approaching_threshold && wayPointsQueue.Count == 0 && state == "moving" )
            {
                state = "approaching";
            }
            
            Quaternion wantedRotation = Quaternion.LookRotation(currentTargetPosition - transform.position, transform.up);
            if (state == "moving" || state == "background") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
        }
        else
        {
            if (!toPark)
            {
                if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
                {
                    if (wayPointsQueue.Count == 0)
                    {
                        waitTimer = 0.0f;
                        waitTime = UnityEngine.Random.Range(1.0f, 2.0f) / vcs.speedMultiplier;
                        state = "operation_point_arrived";
                        return;
                    }
                    else
                    {
                        currentTargetPosition = wayPointsQueue.Dequeue();
                        return;
                    }
                }
                
                Quaternion wantedRotation = Quaternion.LookRotation(currentTargetPosition - transform.position, transform.up);
                //if (state == "moving") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
                transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
                
            }
            else
            {

                if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
                {
                    if (wayPointsQueue.Count > 0)
                    {
                        currentTargetPosition = wayPointsQueue.Dequeue();
                        return;
                    }
                    else
                    {
                        state = "landing_requested";
                        return;
                    }


                }

                if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < approaching_threshold && wayPointsQueue.Count == 0 && state == "moving")
                {
                    //isUTM = false;
                    state = "approaching";
                }

                Quaternion wantedRotation = Quaternion.LookRotation(currentTargetPosition - transform.position, transform.up);
                if (state == "moving") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
                transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
            }
        }
    }

    void Seek()
    {
        // TO-DO: Implement avoidance here
        // Avoidance can be: Globally planned paths + only drone avoidance , Hybrid, Pure local obstacle avoidance
        
    }

    

    private float KMHtoMPS ( float speed )
    {
        return (speed * 1000) / 3600;
    }
   
    public bool UpdateToSystem(Vehicle v)
    {
        VehicleControlSystem system = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        return system.UpdateVehicleStatus(v);
    }
    public bool RegisterToSystem(Vehicle v)
    {
        VehicleControlSystem system = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        return system.Register(v);
    }



    public void SetState(string state)
    {
        this.state = state;
    }
    public string GetState()
    {
        return this.state;
    }
    public void SetOrigin(string origin)
    {
        this.origin = origin;
    }

    public float GetNoise()
    {
        // Returns dB at the noise source
        // TO-DO : Find more realistic function that calculates noise based on speed
        //return noise[4] * Mathf.Pow(speed, 4) + noise[3] * Mathf.Pow(speed, 3) + noise [2] * Mathf.Pow(speed, 2) + Mathf.Pow

        return 80;
    }
                */

}
