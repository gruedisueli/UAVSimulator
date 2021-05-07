using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.DataStructure;
using Assets.Scripts.SimulatorCore;

public abstract class DroneBase : MonoBehaviour
{

    // constants
    public float arrival_threshold { get; set; }
    public float approaching_threshold { get; set; }

    // Vehicle specific information - can be modified in runtime       
    public int id { get; set; }
    public string type { get; set; }
    public int capacity { get; set; }
    public float maxSpeed { get; set; }
    public float yawSpeed { get; set; }
    public float takeOffSpeed { get; set; }
    public float landingSpeed { get; set; }
    public float range { get; set; }


    public List<float> emission;
    public List<float> noise;

    // Analysis/control related information

    public Vector3 currentLocation { get; set; }
    public Vector3 targetPosition { get; set; }
    public float currentSpeed { get; set; }
    public float elevation { get; set; }
    public string origin { get; set; }

    public GameObject currentCommunicationPoint { get; set; }
    public Queue<GameObject> destinationQueue { get; set; }
    public Queue<Vector3> wayPointsQueue { get; set; }

    public float separation { get; set; }
    public Time elapsedTime { get; set; }


    // State machine variable
    // idle, takeoff, move, (land)pending, land, wait
    public string state { get; set; }
    public bool isParked { get; set; }

    
    public int placeInQueue;
    private float landingElevation;
    public bool toPark;
    public bool moveForward;
    public bool isUTM;
    public bool isBackgroundDrone;
    public float waitTimer;
    public float waitTime;

    public event EventHandler<EventArgs> OnDroneTakeOff;
    public event EventHandler<EventArgs> OnDroneParking;
    public EventArgs e;

    private SimulationAnalyzer simulationAnalyzer;


    protected VehicleControlSystem vcs;
    protected int DRONE_LAYERMASK;
    private TrailRenderer tr;
    private SphereCollider sphereCollider;
    private GameObject noiseShpere;
    private Mesh originalMesh;

    // Start is called before the first frame update
    void Start()
    {
        DRONE_LAYERMASK = 1 << 10;
        toPark = false;
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        simulationAnalyzer = GameObject.Find("SimulationCore").GetComponent<SimulationAnalyzer>();
        arrival_threshold = 0.5f;
        destinationQueue = new Queue<GameObject>();
        tr = gameObject.GetComponent<TrailRenderer>();
        sphereCollider = gameObject.GetComponent<SphereCollider>();
        isParked = true;
        originalMesh = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh;


        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            vcs.OnNoiseSphereToggle += NoiseSphereToggleHandler;
            noiseShpere = this.gameObject.transform.GetChild(1).gameObject;
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
            noiseShpere.transform.localScale = new Vector3(currentSpeed / 4.0f, currentSpeed / 4.0f, currentSpeed / 4.0f);
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


    protected virtual Queue<Vector3> GetWayPointsToNextDestination()
    {
        foreach(Corridor c in vcs.sceneManager.network.outEdges[currentCommunicationPoint])
        {
            if ( c.destination.Equals(destinationQueue.Peek()) ) return (new Queue<Vector3>(c.wayPoints));
        }
        return new Queue<Vector3>();
    }

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

    // GetNextAction() implements state machine
    // Below corresponds to the one for CorridorDrone
    // For LowAltitudeDrone, land granting procedure gets slightly different
    // (Refer to LowAltitudeDrone.cs - GetNextAction() )
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

    protected void ParkEvent()
    {
        OnDroneParking?.Invoke(gameObject, e);
        simulationAnalyzer.SendMessage("RemoveFlyingDrone", this.gameObject);
        gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
        isParked = true;
    }
    protected void TakeOffEvent()
    {
        if (isParked)
        {
            gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            OnDroneTakeOff?.Invoke(gameObject, e);
            simulationAnalyzer.SendMessage("AddFlyingDrone", this.gameObject);
            isParked = false;
        }
        targetPosition = wayPointsQueue.Dequeue();
        currentSpeed = takeOffSpeed;
        state = "takeoff";
    }
    protected virtual void Pending()
    {
        state = "pending";
        currentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
    }

    public virtual void LandGranted()
    {
        state = "land";
    }
    public virtual void TakeoffGranted()
    {
        TakeOffEvent();
    }
    

    protected virtual void ReserveNearestParking()
    {
    }

    protected float KMHtoMPS(float speed)
    {
        return (speed * 1000) / 3600;
    }

    #region Public Methods

    public float GetNoise()
    {
        // Returns dB at the noise source
        // TO-DO : Find more realistic function that calculates noise based on speed
        //return noise[4] * Mathf.Pow(speed, 4) + noise[3] * Mathf.Pow(speed, 3) + noise [2] * Mathf.Pow(speed, 2) + Mathf.Pow

        return 80;
    }

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

    public void HideMesh()
    {
        MeshRenderer mr = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        mr.enabled = false;
    }
    public void ShowMesh()
    {
        MeshRenderer mr = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        mr.enabled = true;
    }
    #endregion


    #region Private Methods

    private void NoiseSphereToggleHandler(bool toggle)
    {
        MeshRenderer mr = this.gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>();
        if ( toggle )
        {
            mr.enabled = true;
        }
        else
        {
            mr.enabled = false;
        }

    }

    private void MeshSwapHandler (bool toggle, Mesh m)
    {
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
