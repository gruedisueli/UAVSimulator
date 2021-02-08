using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;

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

    
    
    
    VehicleControlSystem vcs;

    #region Constructors

    public void SetVehicleInfo(string type, int capacity, float range, float maxSpeed, float yawspeed, float takeOffSpeed, float landingSpeed, List<float> emission, List<float> noise)
    {
        this.type = type;
        this.capacity = capacity;
        this.maxSpeed = maxSpeed;
        this.yawSpeed = yawspeed;
        this.takeOffSpeed = takeOffSpeed;
        this.landingSpeed = landingSpeed;
        this.emission = emission;
        this.noise = noise;
        this.range = range;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        arrival_threshold = 0.5f;
        wayPoints = new List<Vector3>();
        wayPointsQueue = new Queue<Vector3>();
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        elevation = Mathf.NegativeInfinity;
        toPark = false;
        moveForward = false;
    }

    // Update is called once per frame
    void Update()
    {
        currentLocation = gameObject.transform.position;
        if ( state == "takeoff_requested" )
        {

        }
        else if ( state == "takeoff_granted" )
        {
            currentSpeed = takeOffSpeed * vcs.speedMultiplier;
            if (elevation < 0.0f)
            {
                if (destination.Count > 0) elevation = vcs.GetElevation(currentPoint, destination.Peek());
                else
                {
                    GameObject reservedParking = vcs.ReserveNearestAvailableParking(gameObject);
                    destination.Enqueue(reservedParking);
                    elevation = vcs.GetElevation(currentPoint, destination.Peek());
                    toPark = true;
                }
            }
            state = "taking_off";
        }
        else if ( state == "taking_off")
        {
            TakeOff();
        }
        else if ( state == "move_ready" )
        {
            
            GameObject currentDestination = destination.Dequeue();            
            Vector3 currentDestinationVector = currentDestination.transform.position;
            currentDestinationVector.y = elevation;

            wayPoints = vcs.FindPath(gameObject.transform.position, currentDestinationVector, 5);
            wayPointsQueue = new Queue<Vector3>();
            foreach (Vector3 v in wayPoints)
            {
                wayPointsQueue.Enqueue(v);
            }
            currentTargetPosition = wayPointsQueue.Dequeue();

            //Debug
            if (Mathf.Abs(currentTargetPosition.x) > 10000 || Mathf.Abs(currentTargetPosition.z) > 10000)
            {
                int a;
                a = 0;
            }
            //~Debug


            currentSpeed = maxSpeed * vcs.speedMultiplier;
            currentPoint = currentDestination;
            state = "moving";           
        }
        else if ( state == "moving" )
        {
            MoveAlong();
        }
        else if ( state == "approaching" )
        {
            Vector3 standbyPosition = new Vector3();
            if (!toPark)
            {
                DronePortControl dp = currentPoint.GetComponent<DronePortControl>();
                if(!dp.queue.Contains(gameObject)) dp.queue.Enqueue(gameObject);
                standbyPosition = dp.GetStandbyPosition(gameObject);
            }
            else
            {
                Parking p = currentPoint.GetComponent<Parking>();
                if (!p.queue.Contains(gameObject)) p.queue.Enqueue(gameObject);
                standbyPosition = p.GetStandbyPosition(gameObject);
            }
            
            state = "registeredInQueue";
            currentTargetPosition = standbyPosition;
        }
        else if ( state == "registeredInQueue" )
        {
            MoveAlong();
        }
        else if ( state == "landing_requested")
        {
            currentSpeed = landingSpeed * vcs.speedMultiplier;
            if (moveForward == true)
            {
                state = "registeredInQueue";
            }
            // When assigned new standby position
        }
        else if ( state == "landing_granted" )
        {
            moveForward = false;
            //currentSpeed = landingSpeed * vcs.speedMultiplier;
            Land();
        }

        //UpdateToSystem(this);
        //if ( destination.Count > 0 )  nextDestination = destination.Peek();
        
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
                //Debug
                if (Mathf.Abs(currentTargetPosition.x) > 10000 || Mathf.Abs(currentTargetPosition.z) > 10000)
                {
                    int a;
                    a = 0;
                }
                //~Debug

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
        if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < approaching_threshold && wayPointsQueue.Count == 0 && state == "moving" )
        {
            state = "approaching";
        }
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
        else
        {
            Quaternion wantedRotation = Quaternion.LookRotation(currentTargetPosition - transform.position, transform.up);
            if ( state == "moving" ) transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
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
   
    
}
