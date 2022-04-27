using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;
using Assets.Scripts.SimulatorCore;

/// <summary>
/// Base class from which all drones are derived.
/// </summary>
public abstract class DroneBase : MonoBehaviour
{
    public event EventHandler<EventArgs> OnDroneTakeOff;
    public event EventHandler<EventArgs> OnDroneParking;
    public EventArgs e;
    public float ArrivalThreshold { get; private set; }//distance within which drone should notify destination of arrival
    public float ApproachingThreshold { get; private set; }
    public string Id { get; } = Guid.NewGuid().ToString().Substring(0, 10);
    public string DroneType { get; private set; }
    public int Capacity { get; private set; }
    public float MaxSpeed { get; private set; }
    public float YawSpeed { get; private set; }
    public float TakeOffSpeed { get; private set; }
    public float LandingSpeed { get; private set; }
    public float SoundAtSourceDb { get; private set; }
    private float _currentNoiseDb = 0;

    public float CurrentNoiseDb
    {
        get => _currentNoiseDb;
    }
    private float _noiseRadius = 0;

    public float NoiseRadius
    {
        get => _noiseRadius;
    }

    //public float range { get; private set; }

    //public List<float> emission;
    //public List<float> noise;

    // Analysis/control related information

    //public Vector3 currentLocation { get; set; }//where the drone is right now.
    public Vector3 TargetPosition { get; set; }//the next point the drone is going to. may be a destination or waypoint. it depends on situation.
    public float CurrentSpeed { get; protected set; }
    public float Elevation { get; private set; }
    //public string origin { get; set; }

    public GameObject CurrentCommunicationPoint { get; set; }//point that we are either taking off from or landing at.
    public Queue<GameObject> DestinationQueue { get; set; }//list of all points that a drone may go to during course of its trip.
    public Queue<Vector3> WayPointsQueue { get; set; }//list of just the points at at takeoff or landing to get drone to the landing pad or into the air. 

    //public float separation { get; set; }
    //public Time elapsedTime { get; set; }


    // State machine variable
    // idle, takeoff, move, (land)pending, land, wait
    public string State { get; set; }
    public bool IsParked { get; protected set; }

    
    //public int placeInQueue;
    //private float landingElevation;
    public bool ToPark { get; set; }
    //public bool moveForward;
    //public bool isUTM;
    public bool IsBackgroundDrone { get; private set; }
    public float WaitTimer { get; protected set; }
    public float WaitTime { get; private set; }
    public GameObject Clone2d { get; set; } = null;
    public GameObject SelectionCircle { get; set; } = null;

    private SimulationAnalyzer _simulationAnalyzer;
    protected VehicleControlSystem _vcs;
    protected int _droneLayermask;
    private TrailRenderer _trailRenderer;
    //private SphereCollider sphereCollider;
    private GameObject _noiseSphere;
    private Mesh _originalMesh = null;


    void Awake()
    {
        _vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        SetNoise(0);
    }

    void Start()
    {
        _droneLayermask = LayerMask.GetMask("Vehicle");// 1 << 10;
        ToPark = false;
        _simulationAnalyzer = GameObject.Find("SimulationCore").GetComponent<SimulationAnalyzer>();
        ArrivalThreshold = 0.5f;
        DestinationQueue = new Queue<GameObject>();
        _trailRenderer = gameObject.GetComponent<TrailRenderer>();
        //sphereCollider = gameObject.GetComponent<SphereCollider>();
        IsParked = true;
        if (!_vcs.TEMPORARY_IsRegionView) _originalMesh = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh;

        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            _vcs.OnNoiseSphereToggle += NoiseSphereToggleHandler;
            int i = _vcs.TEMPORARY_IsRegionView ? 0 : 1;
            _noiseSphere = this.gameObject.transform.GetChild(i).gameObject;
        }
        _vcs.OnSimplifiedMeshToggle += MeshSwapHandler;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_vcs.playing) return;
        //sphereCollider.center = gameObject.transform.position;

        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            _noiseSphere.transform.localScale = new Vector3(NoiseRadius * 2, NoiseRadius * 2, NoiseRadius * 2);
        }

        Elevation = this.gameObject.transform.position.y;

        if (State == "takeoff" || State == "move" || State == "land" )
        {
            if (Vector3.Distance(gameObject.transform.position, TargetPosition) < ArrivalThreshold)
            {
                if (WayPointsQueue.Count == 0)
                {
                    GetNextAction();
                }
                else
                {
                    TargetPosition = WayPointsQueue.Dequeue();
                }
                return;
            }
            Move();
        }
        else if ( State == "wait" )
        {
            WaitTimer += Time.deltaTime;            
            if ( WaitTimer > WaitTime )
            {
                GetNextAction();
            }
        }
        else if ( State == "background")
        { 
            Move();
        }

        if (_vcs.trailVisualization) _trailRenderer.enabled = true;
        else _trailRenderer.enabled = false;
    }

    /// <summary>
    /// @Eunu. comment. Are these "waypoints" different from takeoff/landing waypoints?
    /// </summary>
    protected virtual Queue<Vector3> GetWayPointsToNextDestination()
    {
        foreach(Corridor c in _vcs.DroneNetwork.outEdges[CurrentCommunicationPoint])
        {
            if ( c.destination.Equals(DestinationQueue.Peek()) ) return (new Queue<Vector3>(c.wayPoints));
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
        //     : Corridor drones only live in corridors so it only needs to check the
        // to front
        //     : Restriction zone avoidance is already taken into account by strategic deconfliction
        // (2) Low-altitude drones (w/ or w/o strategic deconfliction, w/ tactical deconfliction)
        //     : Low-altitude drones fly freely in space so it needs to dynamically avoid other drones, buildings, etc.
        //     : Needs dynamic tactical deconfliction
        
        Quaternion wantedRotation = Quaternion.LookRotation(TargetPosition - transform.position, transform.up);
        if (State == "move") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * YawSpeed);
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, CurrentSpeed * Time.deltaTime * EnvironManager.Instance.Environ.SimSettings.SimulationSpeedMultiplier);
    }

    /// <summary>
    /// GetNextAction() implements state machine
    /// </summary>
    protected virtual void GetNextAction()
    {
        if (State == "wait")
        {
            State = "idle";
        }
        else if (State == "takeoff")
        {
            WayPointsQueue = GetWayPointsToNextDestination();
            CurrentSpeed = MaxSpeed;
            CurrentCommunicationPoint.SendMessage("FreeUp");
            CurrentCommunicationPoint = DestinationQueue.Dequeue();
            State = "move";
        }
        else if (State == "move")
        {
            CurrentSpeed = 0;
            Pending();
            
        }
        else if ( State == "land" )
        {
            if ( DestinationQueue.Count == 0 )
            {
                State = "idle";
                TrafficControl tc = CurrentCommunicationPoint.GetComponent<TrafficControl>();
                tc.FreeUp();
                ParkEvent();
            }
            else
            {
                CurrentSpeed = 0;
                State = "wait";
                WaitTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Event called when drone parks.
    /// </summary>
    protected void ParkEvent()
    {
        CurrentSpeed = 0;
        OnDroneParking?.Invoke(gameObject, e);
        _simulationAnalyzer.SendMessage("RemoveFlyingDrone", this);
        if (!_vcs.TEMPORARY_IsRegionView) gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
        else if (Clone2d != null)
        {
            var image = Clone2d.GetComponent<Image>();
            if (image != null) image.enabled = false;
        }
        IsParked = true;
        SetNoise(0);
    }

    /// <summary>
    /// Event called when drone takes off
    /// </summary>
    protected void TakeOffEvent()
    {
        if (IsParked)
        {
            if (!_vcs.TEMPORARY_IsRegionView) gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            else if (Clone2d != null)
            {
                var image = Clone2d.GetComponent<Image>();
                if (image != null) image.enabled = true;
            }
            OnDroneTakeOff?.Invoke(gameObject, e);
            _simulationAnalyzer.SendMessage("AddFlyingDrone", this);
            IsParked = false;
        }
        TargetPosition = WayPointsQueue.Dequeue();
        CurrentSpeed = TakeOffSpeed;
        State = "takeoff";
        SetNoise(SoundAtSourceDb);
    }

    /// <summary>
    /// Changes state machine to a pending state, where drone is waiting for further instructions. @Eunu correct?
    /// </summary>
    protected virtual void Pending()
    {
        CurrentSpeed = 0;
        State = "pending";
        CurrentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
    }

    /// <summary>
    /// Changes state machine to landing state.
    /// </summary>
    public virtual void LandGranted()
    {
        State = "land";
    }

    /// <summary>
    /// Called when we grant take off to drone.
    /// </summary>
    public virtual void TakeoffGranted()
    {
        TakeOffEvent();
    }
    
    protected virtual void ReserveNearestParking()
    {
    }

    private void OnDestroy()
    {
        if (this is CorridorDrone || this is LowAltitudeDrone)
        {
            _vcs.OnNoiseSphereToggle -= NoiseSphereToggleHandler;
        }
        Clone2d?.Destroy();
        SelectionCircle?.Destroy();
    }

    #region Public Methods

    /// <summary>
    /// Initializes the properties of this object.
    /// </summary>
    public void Initialize(DroneSettings droneSettings, string state, bool isBackgroundDrone)
    {
        DroneType = droneSettings.DroneType;
        Capacity = droneSettings.Capacity;
        MaxSpeed = droneSettings.MaxSpeed_MPS;
        YawSpeed = droneSettings.YawSpeed;
        TakeOffSpeed = droneSettings.TakeOffSpeed_MPS;
        LandingSpeed = droneSettings.LandingSpeed_MPS;
        WaitTime = droneSettings.WaitTime_S;
        SoundAtSourceDb = droneSettings.SoundAtSource_Decibels;
        State = state;
        IsBackgroundDrone = isBackgroundDrone;
    }

    /// <summary>
    /// Hides this drone in view
    /// </summary>
    public void HideMesh()
    {
        if (!_vcs.TEMPORARY_IsRegionView)
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
        if (!_vcs.TEMPORARY_IsRegionView)
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
        int i = _vcs.TEMPORARY_IsRegionView ? 0 : 1;
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
        if (_vcs.TEMPORARY_IsRegionView) return;
        MeshFilter mf = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        if(toggle)
        {
            mf.mesh = m;
        }
        else
        {
            mf.mesh = _originalMesh;
        }
    }

    /// <summary>
    /// Noise radius is defined by distance from source at which sound level is equal to acceptable threshold, as defined by simulation settings
    /// For formulas, source was https://www.omnicalculator.com/physics/distance-attenuation
    /// </summary>
    private void SetNoise(float noiseDb)
    {
        _currentNoiseDb = noiseDb;
        //SPL₂ = SPL₁ - 20 * log (R₂ / R₁)
        //where: SPL₁ = noise at R₁
        //SPL₂ = acceptable threshold
        //R₁ = dist at SPL₁
        //R₂ = unknown radius where we hit acceptable threshold
        //rearrange formula for unknowns:
        //SPL₂ = SPL₁ - 20 * log (R₂ / R₁)
        //SPL₂ - SPL₁ = -20 * log (R₂ / R₁)
        //(SPL₂ - SPL₁)/(-20) = log (R₂ / R₁)
        //10^((SPL₂ - SPL₁)/(-20)) = R₂ / R₁
        //10^((SPL₂ - SPL₁)/(-20))*R₁ = R₂

        //from https://www.chem.purdue.edu/chemsafety/Training/PPETrain/dblevels.htm
        //we see that a helicopter at 100ft away is about 100db
        //to find sound level at source: SPL₂ = SPL₁ - 20 * log (R₂ / R₁)
        //SPL₂ = 100 - 20 * log (1 / 100)
        //SPL₂ = 140dB

        _noiseRadius = (float)Math.Pow(10, ((double)EnvironManager.Instance.Environ.SimSettings.AcceptableNoiseThreshold_Decibels - (double)noiseDb) / (-20));
    }
    #endregion
}
