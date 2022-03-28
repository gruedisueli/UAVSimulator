using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.DataStructure;
using Assets.Scripts.SimulatorCore;

/// <summary>
/// Base class from which all drones are derived.
/// </summary>
public abstract class DroneBase : MonoBehaviour
{

    // constants
    //@Eunu how are these different?
    public float arrival_threshold { get; set; }//distance within which drone should notify destination of arrival. @Eunu correct?
    public float approaching_threshold { get; set; }//@Eunu comment

    // Vehicle specific information - can be modified in runtime       
    public string id { get; set; } = Guid.NewGuid().ToString().Substring(0, 10);
    public string type { get; set; }
    public int capacity { get; set; }
    public float maxSpeed { get; set; }
    public float yawSpeed { get; set; }
    public float takeOffSpeed { get; set; }
    public float landingSpeed { get; set; }
    public float range { get; set; }


    public List<float> emission;//@Eunu comment. List = History tracker?
    public List<float> noise;//@Eunu comment. List = History tracker?

    // Analysis/control related information

    public Vector3 currentLocation { get; set; }//where the drone is right now. @Eunu, correct?
    public Vector3 targetPosition { get; set; }//the next point the drone is going to. may be a destination or waypoint. it depends on situation. @Eunu, correct?
    public float currentSpeed { get; set; }
    public float elevation { get; set; }
    public string origin { get; set; }//@Eunu comment.

    public GameObject currentCommunicationPoint { get; set; }//point that we are either taking off from or landing at. @Eunu correct?
    public Queue<GameObject> destinationQueue { get; set; }//list of all points that a drone may go to during course of its trip. @Eunu correct?
    public Queue<Vector3> wayPointsQueue { get; set; }//list of just the points at at takeoff or landing to get drone to the landing pad or into the air. @Eunu correct?

    public float separation { get; set; }//@Eunu comment
    public Time elapsedTime { get; set; }//@Eunu comment


    // State machine variable
    // idle, takeoff, move, (land)pending, land, wait
    public string state { get; set; }
    public bool isParked { get; set; }

    
    public int placeInQueue;//@Eunu comment. I am not sure if we use this?
    private float landingElevation;//@Eunu comment.
    public bool toPark;//@Eunu comment
    public bool moveForward;//@Eunu comment
    public bool isUTM;//@Eunu comment
    public bool isBackgroundDrone;
    public float waitTimer;//@Eunu comment
    public float waitTime;//@Eunu comment

    public event EventHandler<EventArgs> OnDroneTakeOff;
    public event EventHandler<EventArgs> OnDroneParking;
    public EventArgs e;

    private SimulationAnalyzer simulationAnalyzer;


    protected VehicleControlSystem vcs;
    protected int DRONE_LAYERMASK;
    private TrailRenderer tr;
    private SphereCollider sphereCollider;
    private GameObject noiseShpere;
    private Mesh originalMesh = null;
    public GameObject Clone2d { get; set; } = null;
    public GameObject SelectionCircle { get; set; } = null;

    void Awake()
    {
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
    }

    void Start()
    {
        DRONE_LAYERMASK = LayerMask.GetMask("Vehicle");// 1 << 10;
        toPark = false;
        simulationAnalyzer = GameObject.Find("SimulationCore").GetComponent<SimulationAnalyzer>();
        arrival_threshold = 0.5f;
        destinationQueue = new Queue<GameObject>();
        tr = gameObject.GetComponent<TrailRenderer>();
        sphereCollider = gameObject.GetComponent<SphereCollider>();
        isParked = true;
        if (!vcs.TEMPORARY_IsRegionView) originalMesh = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh;

        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            vcs.OnNoiseSphereToggle += NoiseSphereToggleHandler;
            int i = vcs.TEMPORARY_IsRegionView ? 0 : 1;
            noiseShpere = this.gameObject.transform.GetChild(i).gameObject;
        }
        vcs.OnSimplifiedMeshToggle += MeshSwapHandler;
    }

    // Update is called once per frame
    void Update()
    {
        if (!vcs.playing) return;
        //sphereCollider.center = gameObject.transform.position;

        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            noiseShpere.transform.localScale = new Vector3(currentSpeed / 4.0f, currentSpeed / 4.0f, currentSpeed / 4.0f); //@Eunu comment on how we size the noise sphere
        }

        elevation = this.gameObject.transform.position.y;

        if (state == "takeoff" || state == "move" || state == "land" )
        {
            if (Vector3.Distance(gameObject.transform.position, targetPosition) < arrival_threshold)
            {
                if (wayPointsQueue.Count == 0)
                {
                    GetNextAction();
                }
                else
                {
                    targetPosition = wayPointsQueue.Dequeue();
                }
                return;
            }
            Move();
        }
        else if ( state == "wait" )
        {
            waitTimer += Time.deltaTime;            
            if ( waitTimer > waitTime )
            {
                GetNextAction();
            }
        }
        else if ( state == "background")
        { 
            Move();
        }

        if (vcs.trailVisualization) tr.enabled = true;
        else tr.enabled = false;
    }

    /// <summary>
    /// @Eunu. comment. Are these "waypoints" different from takeoff/landing waypoints?
    /// </summary>
    protected virtual Queue<Vector3> GetWayPointsToNextDestination()
    {
        foreach(Corridor c in vcs.DroneNetwork.outEdges[currentCommunicationPoint])
        {
            if ( c.destination.Equals(destinationQueue.Peek()) ) return (new Queue<Vector3>(c.wayPoints));
        }
        return new Queue<Vector3>();
    }

    /// <summary>
    /// Move instructions common to all drones can be included here, or more specific ones in move() overrides of specific drone types.
    /// </summary>
    protected virtual void Move()
    {
        // Move() can be different between:
        // (1) Corridor drones (w/ strategic deconfliction, w/o tactical deconfliction)
        //     : Corridor drones only live in corridors so it only needs to check the separation to front
        //     : Restriction zone avoidance is already taken into account by strategic deconfliction
        // (2) Low-altitude drones (w/ or w/o strategic deconfliction, w/ tactical deconfliction)
        //     : Low-altitude drones fly freely in space so it needs to dynamically avoid other drones, buildings, etc.
        //     : Needs dynamic tactical deconfliction
        
        Quaternion wantedRotation = Quaternion.LookRotation(targetPosition - transform.position, transform.up);
        if (state == "move") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime * vcs.speedMultiplier);
    }

    /// <summary>
    /// GetNextAction() implements state machine
    /// Below corresponds to the one for CorridorDrone @Eunu, if this is for the corridor drone, should we move it?.
    /// For LowAltitudeDrone, land granting procedure gets slightly different @Eunu see question above.
    /// (Refer to LowAltitudeDrone.cs - GetNextAction() ) @Eunu same question as above.
    /// </summary>
    protected virtual void GetNextAction()
    {
        if (state == "wait")
        {
            state = "idle";
        }
        else if (state == "takeoff")
        {
            wayPointsQueue = GetWayPointsToNextDestination();
            currentSpeed = maxSpeed;
            currentCommunicationPoint.SendMessage("FreeUp");
            currentCommunicationPoint = destinationQueue.Dequeue();
            state = "move";
        }
        else if (state == "move")
        {
            Pending();
            
        }
        else if ( state == "land" )
        {
            if ( destinationQueue.Count == 0 )
            {
                state = "idle";
                TrafficControl tc = currentCommunicationPoint.GetComponent<TrafficControl>();
                tc.FreeUp();
                ParkEvent();
            }
            else
            {
                state = "wait";
                waitTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Event called when drone parks.
    /// </summary>
    protected void ParkEvent()
    {
        OnDroneParking?.Invoke(gameObject, e);
        simulationAnalyzer.SendMessage("RemoveFlyingDrone", this.gameObject);
        if (!vcs.TEMPORARY_IsRegionView) gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
        else if (Clone2d != null)
        {
            var image = Clone2d.GetComponent<Image>();
            if (image != null) image.enabled = false;
        }
        isParked = true;
    }

    /// <summary>
    /// Event called when drone takes off
    /// </summary>
    protected void TakeOffEvent()
    {
        if (isParked)
        {
            if (!vcs.TEMPORARY_IsRegionView) gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            else if (Clone2d != null)
            {
                var image = Clone2d.GetComponent<Image>();
                if (image != null) image.enabled = true;
            }
            OnDroneTakeOff?.Invoke(gameObject, e);
            simulationAnalyzer.SendMessage("AddFlyingDrone", this.gameObject);
            isParked = false;
        }
        targetPosition = wayPointsQueue.Dequeue();
        currentSpeed = takeOffSpeed;
        state = "takeoff";
    }

    /// <summary>
    /// Changes state machine to a pending state, where drone is waiting for further instructions. @Eunu correct?
    /// </summary>
    protected virtual void Pending()
    {
        state = "pending";
        currentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
    }

    /// <summary>
    /// Changes state machine to landing state.
    /// </summary>
    public virtual void LandGranted()
    {
        state = "land";
    }

    /// <summary>
    /// Called when we grant take off to drone.
    /// </summary>
    public virtual void TakeoffGranted()
    {
        TakeOffEvent();
    }
    
    /// <summary>
    /// @Eunu comment
    /// </summary>
    protected virtual void ReserveNearestParking()
    {
    }

    /// <summary>
    /// @Eunu comment. Is this kilometers/hr to meters/sec? Shouldn't that be: speed(km/hr) * 1000(m/km) / 216,000 (s/hr) ?
    /// </summary>
    protected float KMHtoMPS(float speed)
    {
        return (speed * 1000) / 3600;
    }

    private void OnDestroy()
    {
        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            vcs.OnNoiseSphereToggle -= NoiseSphereToggleHandler;
        }
        Clone2d?.Destroy();
        SelectionCircle?.Destroy();
    }

    #region Public Methods

    /// <summary>
    /// @Eunu comment. I guess we have some work to do here on making this more realistic?
    /// </summary>
    /// <returns></returns>
    public float GetNoise()
    {
        // Returns dB at the noise source
        // TO-DO : Find more realistic function that calculates noise based on speed
        //return noise[4] * Mathf.Pow(speed, 4) + noise[3] * Mathf.Pow(speed, 3) + noise [2] * Mathf.Pow(speed, 2) + Mathf.Pow

        return 80;
    }

    /// <summary>
    /// Initializes the properties of this object.
    /// </summary>
    public void Initialize(string type, int capacity, float range, float maxSpeed, float yawspeed, float takeOffSpeed, float landingSpeed, List<float> emission, List<float> noise, string state)
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
        this.state = state;
    }

    /// <summary>
    /// Hides this drone in view
    /// </summary>
    public void HideMesh()
    {
        if (!vcs.TEMPORARY_IsRegionView)
        {
            MeshRenderer mr = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
            mr.enabled = false;
        }
        else if (Clone2d != null)
        {
            var image = Clone2d.GetComponent<Image>();
            if (image != null) image.enabled = false;
        }
    }

    /// <summary>
    /// Shows this drone in view.
    /// </summary>
    public void ShowMesh()
    {
        if (!vcs.TEMPORARY_IsRegionView)
        {
            MeshRenderer mr = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
        else if (Clone2d != null)
        {
            var image = Clone2d.GetComponent<Image>();
            if (image != null) image.enabled = true;
        }
    }
    #endregion


    #region Private Methods

    /// <summary>
    /// Turns on and off the noise sphere visualization
    /// </summary>
    private void NoiseSphereToggleHandler(bool toggle)
    {
        int i = vcs.TEMPORARY_IsRegionView ? 0 : 1;
        MeshRenderer mr = this.gameObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>();
        if ( toggle )
        {
            mr.enabled = true;
        }
        else
        {
            mr.enabled = false;
        }

    }

    /// <summary>
    /// Swaps the mesh rendered on this drone.
    /// </summary>
    private void MeshSwapHandler (bool toggle, Mesh m)
    {
        if (vcs.TEMPORARY_IsRegionView) return;
        MeshFilter mf = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        if(toggle)
        {
            mf.mesh = m;
        }
        else
        {
            mf.mesh = originalMesh;
        }
    }
    #endregion
}
