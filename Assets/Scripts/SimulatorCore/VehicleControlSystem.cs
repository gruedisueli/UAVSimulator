using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Numerics;
using Assets.Scripts.SimulatorCore;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;
using Assets.Scripts.UI;
using Assets.Scripts;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI.Tools;

using DelaunatorSharp;
using JetBrains.Annotations;
using TMPro;
using GameObject = UnityEngine.GameObject;
using Quaternion = UnityEngine.Quaternion;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


/// <summary>
/// Sort of a main control system for the entire fleet of drones in the simulation.
/// </summary>
public class VehicleControlSystem : MonoBehaviour
{
    public bool IsNetworkValid { get; private set; } = false;
    public bool TEMPORARY_IsRegionView = true;
    public SimulationAnalyzer _simulationAnalyzer;
    public float MIN_DRONE_RANGE;
    /// <summary>
    /// Initial scale for simulation. (vestigial and probably can be removed)
    /// </summary>
    public float DRONE_SCALE = 1.0f;
    /// <summary>
    /// For inflating size of drone in viewport. Does not affect size of drone used in simulation for collisions, etc.
    /// </summary>
    public float scaleMultiplier = 5.0f;
    public Canvas _canvas;
    private Camera _mainCamera;
    private float _camStartHeight = 0;

    #region Private Variables

    private bool _haveCorridorSim = false;
    private bool _haveLowAltSim = false;
    private float watch;

    // Visualization related params
    public bool playing;
    public bool _networkVisible = false;
    public bool privacyVisualization;
    public bool landingCorridorVisualization;
    public bool demographicVisualization;
    public bool trailVisualization;

    private bool _simplifiedMeshToggle;
    public delegate void OnSimplifiedMeshToggleDelegate(bool toggle, Mesh m);
    public event OnSimplifiedMeshToggleDelegate OnSimplifiedMeshToggle;
    public Mesh simplifiedMesh;
    private List<GameObject> _aaoControls = new List<GameObject>();
    public bool simplifiedMeshToggle
    {
        get
        {
            return _simplifiedMeshToggle;
        }
        set
        {
            if (value != _simplifiedMeshToggle && OnSimplifiedMeshToggle != null)
            {
                OnSimplifiedMeshToggle(!simplifiedMeshToggle, simplifiedMesh);
            }
            _simplifiedMeshToggle = value;
        }
    }
    

    private bool _noiseShpereVisualization;

    public delegate void OnNoiseSphereToggleDelegate(bool toggle);
    public event OnNoiseSphereToggleDelegate OnNoiseSphereToggle;
    public bool noiseShpereVisualization
    {
        get
        {
            return _noiseShpereVisualization;
        }
        set
        {
            if (value != _noiseShpereVisualization && OnNoiseSphereToggle != null)
            {
                OnNoiseSphereToggle(!noiseShpereVisualization);
            }
            _noiseShpereVisualization = value; 
        }
    }

    public SceneManagerBase sceneManager;    

    // Background drone related params
    public float[][] mapBounds;

    #endregion

    public DroneInstantiator droneInstantiator;

    /// <summary>
    /// Network of paths drones can follow.
    /// </summary>
    public Assets.Scripts.DataStructure.Network DroneNetwork { get; protected set; } = new Assets.Scripts.DataStructure.Network();
    public List<GameObject> networkLines = new List<GameObject>();
    private Dictionary<Corridor, GameObject> routeLineObject = new Dictionary<Corridor, GameObject>();
    private bool _networkUpdateFlag = false;
    private int _framesSinceNetworkUpdateFlag = 0;

    private List<List<Obstacle>> _obstacleGroupsLowAlt;

    void Awake()
    {
        EnvironManager.Instance.VCS = this;
        playing = false;

        MIN_DRONE_RANGE = 999999.0f;
        sceneManager = FindObjectOfType<SceneManagerBase>(true);
        if (sceneManager == null) Debug.Log("Could not find scene manager component in scene");

        watch = 0.0f;

        mapBounds = UnitUtils.GetRegionExtents();

        droneInstantiator = gameObject.GetComponent<DroneInstantiator>();
        droneInstantiator.Init(this);


        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        simplifiedMesh = cube.GetComponent<MeshFilter>().mesh;
        cube.Destroy();

        UpdateVehicleCount(EnvironManager.Instance.Environ.SimSettings.BackgroundDroneCount);

        _mainCamera = Camera.main;
        _camStartHeight = _mainCamera.transform.position.y;
    }

    // Update is called once per frames
    void Update()
    {
        #region Typical play actions

        if (playing && droneInstantiator.isCorridorDroneInstantiated && droneInstantiator.isLowAltitudeDroneInstantiated)
        {
            watch += Time.deltaTime;
            //periodic check-in
            if (watch > EnvironManager.Instance.Environ.SimSettings.CallGenerationInterval_S / EnvironManager.Instance.Environ.SimSettings.SimulationSpeedMultiplier)
            {
                watch = 0.0f;
                GenerateRandomCalls();
            }
        }

        #endregion
    }

    void FixedUpdate()
    {
        //We have to use a flag because it seems that the colliders for restriction zones only become active after a game frame
        if (_networkUpdateFlag && _framesSinceNetworkUpdateFlag < 2)
        {
            _framesSinceNetworkUpdateFlag++;
        }
        else if (_networkUpdateFlag)
        {
            IsNetworkValid = GenerateNetwork();
            VisualizeNetwork();
            _networkUpdateFlag = false;
        }
    }


    #region Methods

    /// <summary>
    /// Called (from scene manager) whenever we push the button that plays/pauses simulation. If currently playing, pauses simulation, else plays simulation.
    /// </summary>
    public void PlayPause(bool play)
    {
        playing = play;
        _simulationAnalyzer.PlayStop(play);

        if ( playing )
        {
            var e = EnvironManager.Instance.Environ;
            var dPCt = e.DronePorts.Count;
            _haveCorridorSim = false;
            _haveLowAltSim = false;
            foreach (var ps in e.ParkingStructures)
            {
                if (ps.Value.Type.Contains("LowAltitude"))
                {
                    _haveLowAltSim = true;
                }
                else if (dPCt >= 3)
                {
                    _haveCorridorSim = true;
                }
            }

            droneInstantiator.ClearDrones();
            droneInstantiator.InstantiateDrones(sceneManager, scaleMultiplier, _canvas);
            var simSettings = EnvironManager.Instance.Environ.SimSettings;
            droneInstantiator.InstantiateBackgroundDrones(sceneManager, simSettings.BackgroundDroneCount, scaleMultiplier, simSettings.BackgoundDroneLowerElev_M, simSettings.BackgroundDroneUpperElev_M, _canvas);
        }
        else
        {
            foreach (var o in _aaoControls)
            {
                o.Destroy();
            }
            _aaoControls.Clear();
            droneInstantiator.ClearDrones();
            foreach (var nL in networkLines)
            {
                var r = nL.GetComponent<LineRenderer>();
                if (r == null)
                {
                    continue;
                }

                r.material = Resources.Load<Material>("Materials/Route");
            }
        }
    }

    /// <summary>
    /// Generates random calls for drone actions (only low-altitude and corridor drones, not background).
    /// </summary>
    public void GenerateRandomCalls()
    {
        int call_type = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 3.0f));
        string call_type_string = call_type <= 0 ? "corridor" : "low-altitude";
        
        if (EnvironManager.Instance.Environ.SimSettings.StrategicDeconfliction.Equals("none"))
        {
            if (call_type_string.Equals("corridor") && _haveCorridorSim)
            {
                List<GameObject> destinations_list = GetNewRandomDestinations();
                GameObject parking = GetNearestAvailableParking(destinations_list[0]);
                if (parking == null)
                {
                    return;
                }
                destinations_list.Add(parking);
                Queue<GameObject> destinations = Route(parking, destinations_list);
                if (destinations == null)
                {
                    Debug.Log("Could not find acceptable path");
                    return;
                }
                GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);
                if (vehicle == null)
                {
                    return;
                }
                CallVehicle(vehicle, parking.GetComponent<ParkingControl>(), destinations);
            }
            else if (_haveLowAltSim) // call_type_string == "low-altitude"
            {
                int withAAO = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 1.0f));
                string withAAOString = withAAO == 0 ? "AAO" : "None";

                if (withAAOString.Equals("AAO")) // generating calls with AAO
                {
                    // TO-DO: Augment vehicle type (make it find the right type) and make it possible to have more than one drone in operation per AAO

                    //just choosing a random parking structure on map to generate a call from, for now.
                    GameObject parking = GetNearestAvailableParking(GetRandomPointXZ(0));
                    if(parking == null)
                    {
                        Debug.Log("No available vehicle");
                        return;
                    }
                    GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);
                    if (vehicle == null)
                    {
                        Debug.Log("No available vehicle");
                        return;
                    }

                    //find a random destination for this drone, within the allowable radius.

                    Vector3 destination = GetRandomPointXZ(EnvironManager.Instance.Environ.SimSettings.LowAltitudeFlightElevation_M, parking.transform.position.x, parking.transform.position.z, EnvironManager.Instance.Environ.SimSettings.LowAltitudeDroneTravelRadius_M);//GetRandomPointXZ(0.0f);
                    int layerMask = 1 << 8;//layer 8 = restriction zones.
                    if (IsPointInsideObject(destination, layerMask))
                    {
                        return;
                    }

                    var standBy = parking.GetComponent<ParkingControl>().parkingInfo.StandbyPosition;
                    var pt0 = new Vector3(parking.transform.position.x, EnvironManager.Instance.Environ.SimSettings.LowAltitudeFlightElevation_M, parking.transform.position.z);

                    int branchCt = 0;
                    if (!BestRouteAroundObstacles(pt0, destination, _obstacleGroupsLowAlt, layerMask, 10, 0, 100, true, 1000, ref branchCt, out var generatedPoints))
                    {
                        Debug.Log("Could not route around obstacles");
                        return;
                    }

                    for (int i = generatedPoints.Count - 1; i >= 0; i--)
                    {
                        generatedPoints.Add(generatedPoints[i]);
                    }
                    generatedPoints.Add(standBy + parking.transform.position);
                    var AAO = new GameObject("AAO_" + vehicle.name);
                    _aaoControls.Add(AAO);
                    AAO.layer = 8;
                    AAOControl aaoCon = AAO.AddComponent<AAOControl>();
                    aaoCon.AddVehicle(vehicle);
                    
                    LowAltitudeDrone vehicleInfo = vehicle.GetComponent<LowAltitudeDrone>();
                    vehicleInfo.SetOperationPoints(new Queue<Vector3>(generatedPoints));
                    CallVehicle(vehicle, parking.GetComponent<ParkingControl>(), null);
                }
            }
        }
        else
        {


        }

    }




    /// <summary>
    /// Gives a specific drone a call to go somewhere.
    /// </summary>
    private void CallVehicle(GameObject vehicle, ParkingControl parking, Queue<GameObject> destinations)
    {
        DroneBase vehicleInfo = vehicle.GetComponent<DroneBase>();
        if(destinations != null) vehicleInfo.DestinationQueue = destinations;
        parking.CallVehecleInParkingStructure(vehicle);
    }

    public float GetElevation(GameObject origin, GameObject destination)
    {
        // TO-DO: Assign elevation according to the simulation rules
        // Now: All 100m to test obstacle avoidance
        if (origin.transform.position.y > 100.0f) return origin.transform.position.y + 50.0f;
        return 152.0f;
    }

    /// <summary>
    /// Gives us a random point on the map.
    /// </summary>
    public Vector3 GetRandomPointXZ(float y)
    {
        
        float x = UnityEngine.Random.Range(mapBounds[0][0], mapBounds[0][1]);
        float z = UnityEngine.Random.Range(mapBounds[1][0], mapBounds[1][1]);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Gives us a random point within some radius of start
    /// </summary>
    public Vector3 GetRandomPointXZ(float y, float centerX, float centerZ, float radius)
    {
        var rPt = UnityEngine.Random.insideUnitCircle * radius;
        rPt += new Vector2(centerX, centerZ);
        return new Vector3(rPt.x, y, rPt.y);
    }

    public GameObject ReserveNearestAvailableParking(GameObject v)
    {
        float min_dist = float.PositiveInfinity;
        GameObject nearest = new GameObject();
        string nearestGuid = "";
        foreach (var key in sceneManager.ParkingStructures.Keys)
        {
            var sPS = sceneManager.ParkingStructures[key];
            var gO = sPS.gameObject;
            if (sPS.ParkingCtrl.RemainingSpots > 0)
            {
                if (Vector3.Distance(gO.transform.position, v.transform.position) < min_dist)
                {
                    nearest = gO;
                    nearestGuid = key;
                    min_dist = Vector3.Distance(gO.transform.position, v.transform.position);
                }
            }
        }
        // DEBUG: different vehicles competeing for a spot
        sceneManager.ParkingStructures[nearestGuid].ParkingCtrl.Reserve(v);
        return nearest;
    }

    /// <summary>
    /// Finds an available vehicle in the parking structure.
    /// </summary>
    public GameObject GetAvailableVehicleinParkingStrcuture(GameObject parkingStructure)
    {
        ParkingControl pC = parkingStructure.GetComponent<ParkingControl>();
        for ( int i = 0; i < pC.VehicleAt.Keys.Count; i++)
        {
            try
            {
                GameObject vehicle_i = pC.VehicleAt.Keys.ElementAt<GameObject>(i);
                DroneBase db = vehicle_i.GetComponent<DroneBase>();
                if (db != null && db.State == "idle" && db.DestinationQueue.Count == 0 && !pC.QueueContains(vehicle_i))
                {
                    return vehicle_i;
                }
            }
            catch
            {
                
            }
        }
        return null;
    }

    /// <summary>
    /// Generates a route from origin through specified destinations. May go through additional intermediate points as needed.
    /// Null on failure.
    /// </summary>
    public Queue<GameObject> Route (GameObject origin, List<GameObject> destinations)
    {
        Queue<GameObject> routed_destinations = new Queue<GameObject>();
        destinations.Insert(0, origin);
        for ( int i = 0; i < destinations.Count - 1; i++ )
        {
            List<GameObject> shortestRoute = Dijkstra(destinations[i], destinations[i + 1], MIN_DRONE_RANGE);
            if (shortestRoute == null) return null;
            shortestRoute.RemoveAt(0);
            foreach (GameObject gO in shortestRoute)
            {
                routed_destinations.Enqueue(gO);
            }
        }
        return routed_destinations;
    }



    /// <summary>
    /// Finds shortest path in corridor network from one point to another.
    /// Assumes a vehicle gets fully charged at each stop 
    /// Null on failure.
    /// </summary>
    List<GameObject> Dijkstra(GameObject origin, GameObject destination, float vehicleRange)
    {
        Dictionary<GameObject, float> distanceTo = new Dictionary<GameObject, float>();
        Dictionary<GameObject, List<GameObject>> pathTo = new Dictionary<GameObject, List<GameObject>>();
        Queue<GameObject> queue = new Queue<GameObject>();

        distanceTo.Add(origin, 0.0f);
        pathTo.Add(origin, new List<GameObject>());
        pathTo[origin].Add(origin);

        bool foundAPath = false;
        queue.Enqueue(origin);
        do
        {
            GameObject currentNode = queue.Dequeue();
            Vector3 currentPoint = currentNode.transform.position;
            foreach (Corridor outEdge in DroneNetwork.outEdges[currentNode])
            {
                GameObject nextNode = outEdge.destination;
                Vector3 nextPoint = nextNode.transform.position;
                if (!distanceTo.ContainsKey(nextNode)) distanceTo.Add(nextNode, float.PositiveInfinity);
                if (Vector3.Distance(currentPoint, nextPoint) < vehicleRange && distanceTo[nextNode] > distanceTo[currentNode] + Vector3.Distance(currentPoint, nextPoint))
                {
                    distanceTo[nextNode] = distanceTo[currentNode] + Vector3.Distance(currentPoint, nextPoint);
                    pathTo[nextNode] = new List<GameObject>(pathTo[currentNode]);
                    pathTo[nextNode].Add(nextNode);
                    if (!nextNode.Equals(destination)) queue.Enqueue(nextNode);
                    else foundAPath = true;
                }
            }
        } while (queue.Count > 0);
        return foundAPath ? pathTo[destination] : null;
    }

    /// <summary>
    /// Gets some random destinations for a CORRIDOR drone.
    /// </summary>
    public List<GameObject> GetNewRandomDestinations()
    {

        Queue<GameObject> routedDestinations = new Queue<GameObject>();
        List<GameObject> landings = new List<GameObject>();
        
        foreach(var sDP in sceneManager.DronePorts.Values)
        {
            if (!sDP.IsPositionValid)
            {
                continue;
            }
            landings.Add(sDP.gameObject);
        }
        List<int> indices = new List<int>();
        List<GameObject> destinationList = new List<GameObject>();
        int value = 0, destinationCount = UnityEngine.Random.Range(1, landings.Count + 1);
        float range = MIN_DRONE_RANGE;

        for (int i = 0; i < destinationCount; i++)
        {
            do
            {
                value = Mathf.RoundToInt(UnityEngine.Random.Range(0, landings.Count));
            } while (indices.Contains(value));
            indices.Add(value);
            destinationList.Add(landings[value]);
        }
        return destinationList;
    }

    /// <summary>
    /// Finds nearest parking structure with an available drone to first destination in call.
    /// </summary>
    public GameObject GetNearestAvailableParking(GameObject firstDestination)
    {
        Vector3 pickUpLocation = firstDestination.transform.position;
        GameObject closestParking = null;

        float minDistance = Mathf.Infinity;
        // For all parking structures
        foreach (var sPS in sceneManager.ParkingStructures.Values)
        {
            if (!sPS.IsPositionValid)
            {
                continue;
            }
            var gO = sPS.gameObject;
            // find the nearest one with parked vehicles
            if (sPS.ParkingCtrl.VehicleAt.Keys.Count > 0 && sPS.ParkingCtrl.queueLength < 3 && !sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
            {
                if (Vector3.Distance(pickUpLocation, gO.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(pickUpLocation, gO.transform.position);
                    closestParking = gO;
                }
            }
            else continue;
        }

        return closestParking;

    }

    /// <summary>
    /// Gets nearest parking structure to center of low-altitude drone path. 
    /// </summary>
    public GameObject GetNearestAvailableParking(Vector3 AAOCenter)
    {
        GameObject closestParking = null;

        float minDistance = Mathf.Infinity;
        // For all parking structures
        foreach (var sPS in sceneManager.ParkingStructures.Values)
        {
            if (!sPS.IsPositionValid)
            {
                continue;
            }
            var gO = sPS.gameObject;
            // find the nearest one with parked vehicles
            if (sPS.ParkingCtrl.VehicleAt.Keys.Count > 0 && sPS.ParkingStructureSpecs.Type.Contains("LowAltitude") && sPS.ParkingCtrl.queueLength < 3)
            {
                if (Vector3.Distance(AAOCenter, gO.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(AAOCenter, gO.transform.position);
                    closestParking = gO;
                }
            }
            else continue;
        }

        return closestParking;

    }

    /// <summary>
    /// Allows user to specify number of background drones in simulation at runtime.
    /// </summary>
    public void UpdateVehicleCount(int count)
    {
        if (playing)
        {
            droneInstantiator.SetBackgroundDroneCt(count);
        }
    }

    /// <summary>
    /// Allows user to toggle trail visualization at runtime.
    /// </summary>
    public void ToggleTrailVisualization(bool toggle)
    {
        trailVisualization = toggle;
    }

    /// <summary>
    /// Allows user to toggle privacy visualization at runtime.
    /// </summary>
    public void TogglePrivacyVisualization(bool toggle)
    {
        privacyVisualization = toggle;
    }

    /// <summary>
    /// Allows user to toggle landing corridor visualization at runtime.
    /// </summary>
    public void ToggleLandingCorridorVisualization(bool toggle)
    {
        landingCorridorVisualization = toggle;
    }

    /// <summary>
    /// Allows user to toggle demographics visualization at runtime.
    /// </summary>
    public void ToggleDemographicVisualization(bool toggle)
    {
        demographicVisualization = toggle;
    }

    /// <summary>
    /// Allows user to toggle noise sphere visualization at runtime
    /// </summary>
    public void ToggleNoiseShpere(bool toggle)
    {
        noiseShpereVisualization = toggle;
    }

    /// <summary>
    /// Allows user to toggle simplified mesh visualization at runtime.
    /// </summary>
    public void ToggleSimplifiedMesh(bool toggle)
    {
        simplifiedMeshToggle = toggle;
    }

    /// <summary>
    /// Rebuilds network without resetting drones.
    /// </summary>
    public void RebuildNetwork()
    {
        _networkUpdateFlag = true;
        _framesSinceNetworkUpdateFlag = 0;
    }

    /// <summary>
    /// Generates data structure for network. False on failure.
    /// </summary>
    private bool GenerateNetwork()
    {
        DroneNetwork = new Assets.Scripts.DataStructure.Network();
        int layerMask = 1 << 8;//layer 8 = restriction zones.
        List<GameObject> points = new List<GameObject>();
        var utmElev = EnvironManager.Instance.Environ.SimSettings.CorridorFlightElevation_M;
        foreach (SceneDronePort sdp in sceneManager.DronePorts.Values)
        {
            var p0 = sdp.gameObject.transform.position;
            var p1 = new Vector3(p0.x, utmElev, p0.z);
            sdp.IsPositionValid = !IsPointInsideObject(p1, layerMask);
            if (sdp.IsPositionValid)
            {
                points.Add(sdp.gameObject);
            }
        }
        foreach (SceneParkingStructure sps in sceneManager.ParkingStructures.Values)
        {
            var p0 = sps.gameObject.transform.position;
            var p1 = new Vector3(p0.x, utmElev, p0.z);
            sps.IsPositionValid = !IsPointInsideObject(p1, layerMask);
            if (!sps.ParkingStructureSpecs.Type.Contains("LowAltitude"))
            {
                if (sps.IsPositionValid)
                {
                    points.Add(sps.gameObject);
                }
            }
        }
        var obstacleGroupsLowerPath = GetGroupedObstacles(1, utmElev, layerMask);
        var utmSep = EnvironManager.Instance.Environ.SimSettings.CorridorSeparationDistance_M;
        var obstacleGroupsUpperPath = GetGroupedObstacles(1, utmElev + utmSep, layerMask);
        _obstacleGroupsLowAlt = GetGroupedObstacles(1, EnvironManager.Instance.Environ.SimSettings.LowAltitudeFlightElevation_M, layerMask);

        if (points.Count < 3) return false;

        IPoint[] vertices = GetVertices(points);
        Delaunator delaunay = new Delaunator(vertices);
        DroneNetwork.vertices = points;
        var corrSpeed = EnvironManager.Instance.Environ.SimSettings.CorridorDroneSettings.MaxSpeed_MPS;
        for (int i = 0; i < delaunay.Triangles.Length; i++)
        {
            if (i > delaunay.Halfedges[i])
            {
                GameObject from = points[delaunay.Triangles[i]];
                GameObject to = points[delaunay.Triangles[nextHalfEdge(i)]];
                Corridor corridor_from_to = new Corridor(from, to, utmElev, corrSpeed);
                Vector3 fromFlat = new Vector3(from.transform.position.x, 0, from.transform.position.z);
                Vector3 toFlat = new Vector3(to.transform.position.x, 0, to.transform.position.z);
                Vector3 fromPosition = fromFlat + Vector3.Normalize(toFlat - fromFlat) * 50;
                Vector3 toPosition = toFlat + Vector3.Normalize(fromFlat - toFlat) * 50;
                Vector3 corridor_from_to_start = new Vector3(fromPosition.x, utmElev, fromPosition.z);
                Vector3 corridor_from_to_end = new Vector3(toPosition.x, utmElev, toPosition.z);



                corridor_from_to_start.y = utmElev;
                corridor_from_to_end.y = utmElev;

                Corridor corridor_to_from = new Corridor(to, from, utmElev + utmSep, corrSpeed);
                Vector3 corridor_to_from_start = new Vector3(toPosition.x, utmElev + utmSep, toPosition.z);
                Vector3 corridor_to_from_end = new Vector3(fromPosition.x, utmElev + utmSep, fromPosition.z);

                int branchCt = 0;
                int maxBranches = 1000;
                int maxDepth = 10;
                float divDist = 100;
                if (!BestRouteAroundObstacles(corridor_from_to_start, corridor_from_to_end, obstacleGroupsLowerPath, layerMask, maxDepth, 0, divDist, false, maxBranches, ref branchCt, out var wayPoints))
                {
                    continue;
                }
                //wayPoints.Insert(0, corridor_from_to_start);
                corridor_from_to.wayPoints = new Queue<Vector3>(wayPoints);

                branchCt = 0;
                if (!BestRouteAroundObstacles(corridor_to_from_start, corridor_to_from_end, obstacleGroupsUpperPath, layerMask, maxDepth, 0, divDist, false, maxBranches, ref branchCt, out wayPoints))
                {
                    continue;
                }
                //wayPoints.Insert(0, corridor_to_from_start);
                corridor_to_from.wayPoints = new Queue<Vector3>(wayPoints);

                DroneNetwork.corridors.Add(corridor_from_to);
                DroneNetwork.corridors.Add(corridor_to_from);
                if (!DroneNetwork.inEdges.ContainsKey(from)) DroneNetwork.inEdges.Add(from, new List<Corridor>());
                if (!DroneNetwork.outEdges.ContainsKey(from)) DroneNetwork.outEdges.Add(from, new List<Corridor>());
                if (!DroneNetwork.inEdges.ContainsKey(to)) DroneNetwork.inEdges.Add(to, new List<Corridor>());
                if (!DroneNetwork.outEdges.ContainsKey(to)) DroneNetwork.outEdges.Add(to, new List<Corridor>());

                DroneNetwork.outEdges[from].Add(corridor_from_to);
                DroneNetwork.inEdges[from].Add(corridor_to_from);
                DroneNetwork.outEdges[to].Add(corridor_to_from);
                DroneNetwork.inEdges[to].Add(corridor_from_to);
            }
        }

        return true;
    }
    public int nextHalfEdge(int e)
    {
        return (e % 3 == 2) ? e - 2 : e + 1;
    }
    public IPoint[] GetVertices(List<GameObject> points)
    {
        List<IPoint> pts = new List<IPoint>();
        foreach (GameObject gO in points)
        {
            Vector3 position = gO.transform.position;
            pts.Add(new Point((double)position.x, (double)position.z));
        }
        return pts.ToArray();
    }

    
    /// <summary>
    /// Finds closest restriction zone in the hit objects, if any. Null if none found.
    /// </summary>
    SceneRestrictionZone GetClosestHitRestrictionZone(Vector3 current, RaycastHit[] hit)
    {
        RaycastHit minHit = hit[0];
        var hitZones = new List<Tuple<SceneRestrictionZone, float>>();
        for (int i = 0; i < hit.Length; i++)
        {
            var h = hit[i];
            var z = h.transform.gameObject.GetComponentInParent<SceneRestrictionZone>();
            if (z == null)
            {
                continue;
            }
            hitZones.Add(Tuple.Create(z, Vector3.Distance(current, h.point)));
        }

        if (hitZones.Count == 0)
        {
            return null;
        }
        hitZones = hitZones.OrderBy(z => z.Item2).ToList();

        return hitZones[0].Item1;
    }

    Vector3 RotateAround(Vector3 point, Vector3 pivot, float angle)
    {
        Vector3 newPoint = point - pivot;
        newPoint = Quaternion.Euler(0, angle, 0) * newPoint;
        newPoint = newPoint + pivot;
        return newPoint;
    }

    private class Obstacle
    {
        public string ObjectId { get; }
        public List<Vector3> InflatedPts { get; }
        public bool IntersectsAnother { get; } = false;
        public Dictionary<string, SceneRestrictionZone> IntersectingObjects { get; }

        public Obstacle(SceneRestrictionZone z, float inflation, float height, int layerMask)
        {
            ObjectId = z.Guid;
            InflatedPts = z.RestrictionZoneSpecs.GetBoundaryPtsAtHeight(height, inflation);
            var intersectingObjects = new Dictionary<string, SceneRestrictionZone>();
            for (int i = 0; i < InflatedPts.Count; i++)
            {
                int j = i < InflatedPts.Count - 1 ? i + 1 : 0;
                var p0 = InflatedPts[i];
                var p1 = InflatedPts[j];

                if (DoesPointPairIntersectAnother(p0, p1, layerMask, out var cols))
                {
                    foreach (var c in cols)
                    {
                        var rZ = c.Value.GetComponentInParent<SceneRestrictionZone>();
                        if (rZ == null)
                        {
                            Debug.Log("Intersected object doesn't contain a restriction zone");
                            continue;
                        }
                        IntersectsAnother = true;

                        if (!intersectingObjects.ContainsKey(rZ.Guid))
                        {
                            intersectingObjects.Add(rZ.Guid, rZ);
                        }
                    }
                }
            }

            IntersectingObjects = intersectingObjects;
        }

        
    }

    /// <summary>
    /// Returns true if vector between these points intersects collider(s), returning all that it intersects
    /// </summary>
    private static bool DoesPointPairIntersectAnother(Vector3 p0, Vector3 p1, int layerMask, out Dictionary<int, GameObject> intersectingObjects)
    {
        intersectingObjects = new Dictionary<int, GameObject>();
        var hits = Physics.RaycastAll(p0, p1 - p0, Vector3.Distance(p0, p1), layerMask);
        if (hits != null && hits.Length > 0)
        {
            foreach (var h in hits)
            {
                var id = h.transform.gameObject.GetInstanceID();
                if (!intersectingObjects.ContainsKey(id))
                {
                    intersectingObjects.Add(id, h.transform.gameObject);
                }
            }
        }

        return intersectingObjects.Count > 0;
    }

    /// <summary>
    /// Returns true if a point is inside an object on the specified layers.
    /// </summary>
    private static bool IsPointInsideObject(Vector3 p, int layerMask)
    {
        var cols = Physics.OverlapSphere(p, 1, layerMask);
        return cols != null && cols.Length > 0;
    }

    /// <summary>
    /// Gets all obstacles, grouped by those that overlap within the specified elevation. Overlap is defined by an inflated point being within a neighboring restriction zone.
    /// </summary>
    private List<List<Obstacle>> GetGroupedObstacles(float inflation, float height, int layerMask)
    {
        var ungroupedObstacles = new Dictionary<string, Obstacle>();
        var sRZs = FindObjectsOfType<SceneRestrictionZone>(true);
        foreach (var z in sRZs)
        {
            if (!EnvironManager.Instance.Environ.RestrictionZones.ContainsKey(z.Guid))
            {
                continue;//this could be a restriction zone around a landing pad, for example.
            }

            var o = new Obstacle(z, inflation, height, layerMask);
            ungroupedObstacles.Add(o.ObjectId, o);
        }

        var oGrps = new List<List<string>>();
        foreach(var kvp in ungroupedObstacles)
        {
            var mainID = kvp.Key;
            var obs = kvp.Value;
            if (!obs.IntersectsAnother)
            {
                oGrps.Add(new List<string>(){mainID});
                continue;
            }
            foreach (var other in obs.IntersectingObjects)
            {
                int existingList = -1;
                for (int z = 0; z < oGrps.Count; z++)
                {
                    var g = oGrps[z];
                    if (g.Contains(mainID))
                    {
                        existingList = z;
                        break;
                    }
                }
                bool foundList = false;
                bool purge = false;
                var otherID = other.Key;
                foreach (var g in oGrps)
                {
                    if (g.Contains(otherID))
                    {
                        foundList = true;
                        if (!g.Contains(mainID))
                        {
                            if (existingList != -1)
                            {
                                foreach (var item in oGrps[existingList])
                                {
                                    if (!g.Contains(item))
                                    {
                                        g.Add(item);
                                    }
                                }

                                purge = true;
                            }
                            else
                            {
                                g.Add(mainID);
                            }

                        }
                        break;
                    }
                }
                if (purge)
                {
                    oGrps.RemoveAt(existingList);
                }
                if (!foundList)
                {
                    oGrps.Add(new List<string>() { mainID, otherID });
                }
            }
        }

        Debug.Log("RZ Groups: ");
        List<List<Obstacle>> obstacleGroups = new List<List<Obstacle>>();
        foreach (var g in oGrps)
        {
            string s = "";
            List<Obstacle> obstacles = new List<Obstacle>();
            foreach (var k in g)
            {
                s += k + ",";
                obstacles.Add(ungroupedObstacles[k]);
            }
            obstacleGroups.Add(obstacles);
            Debug.Log(s);
        }

        return obstacleGroups;
    }

    /// <summary>
    /// Routes curve around obstacles. False on failure.
    /// </summary>
    private bool BestRouteAroundObstacles(Vector3 startPt, Vector3 endPt, List<List<Obstacle>> obstacleGroups, int layerMask, int maxDepth, int currentDepth, float divDist, bool simpleConcavityEscape, int maxBranchCt, ref int currentBranchCt, out List<Vector3> routePts)
    {
        routePts = new List<Vector3>() { startPt };
        if (currentDepth > maxDepth)
        {
            Debug.Log("Max depth exceeded");
            return false;
        }
        if (!ObstacleExists(startPt, endPt, obstacleGroups, layerMask, out var obs))
        {
            routePts.Add(endPt);
            return true;
        }

        currentDepth++;
        var paths = GetPathsAround(startPt, endPt, obs, obstacleGroups, divDist, layerMask, simpleConcavityEscape);
        if (paths == null)
        {
            Debug.Log("No paths around object could be found");
            return false;
        }
        var fixRoutes = new List<List<Vector3>>();
        var goodRoutes = new List<List<Vector3>>();
        foreach (var p in paths)
        {
            //find first edge that intersects an obstacle, if any
            //record the first obstacle
            //record an edge from start of that segment to the destination
            obs = null;
            bool foundObs = false;
            var rPts = new List<Vector3>(routePts);
            for (int v0 = 0; v0 < p.Count - 1; v0++)
            {
                int v1 = v0 + 1;
                if (ObstacleExists(p[v0], p[v1], obstacleGroups, layerMask, out obs))
                {
                    foundObs = true;
                    fixRoutes.Add(rPts);
                    break;
                }
                rPts.Add(p[v1]);
            }
            if (!foundObs)
            {
                var goodRt = new List<Vector3>(routePts);
                for (int i = 1; i < p.Count; i++)
                {
                    goodRt.Add(p[i]);
                }
                goodRoutes.Add(goodRt);
            }
        }
        double bestLen = double.MaxValue;
        bool foundBest = false;
        for (int f = 0; f < fixRoutes.Count; f++)
        {
            currentBranchCt++;
            if (currentBranchCt > maxBranchCt)
            {
                Debug.Log("Branch count exceeded");
                break;
            }
            var toFix = fixRoutes[f];
            if (BestRouteAroundObstacles(toFix[toFix.Count - 1], endPt, obstacleGroups, layerMask, maxDepth, currentDepth, divDist, simpleConcavityEscape, maxBranchCt, ref currentBranchCt, out var rPts))
            {
                var tmpPts = new List<Vector3>(toFix);
                for (int i = 1; i < rPts.Count; i++)
                {
                    tmpPts.Add(rPts[i]);
                }
                tmpPts = CleanupPoints(tmpPts, layerMask, obstacleGroups);
                goodRoutes.Add(tmpPts);
            }
        }
        foreach (var gR in goodRoutes)
        {
            var len = GetPathLength(gR);
            if (len < bestLen)
            {
                foundBest = true;
                bestLen = len;
                routePts = gR;
            }
        }
        if (foundBest)
        {
            return true;
        }

        Debug.Log("Path not found");
        return false;
    }


    /// <summary>
    /// Removes unnecessary kinks from basic paths.
    /// </summary>
    private List<Vector3> CleanupPoints(List<Vector3> pts, int layerMask, List<List<Obstacle>> obstacleGroups)
    {
        var modPts = new List<Vector3>(pts);
        int i = 0;
        int c = 0;
        while (i < modPts.Count - 2 && c < pts.Count)
        {
            c++;
            int k = i + 2;
            if (!ObstacleExists(modPts[i], modPts[k], obstacleGroups, layerMask, out _))
            {
                modPts.RemoveAt(i + 1);
                continue;
            }
            i++;
        }
        return modPts;
    }




    ///// <summary>
    ///// Finds path around restriction zones. False on failure.
    ///// </summary>
    //public bool FindPath(Vector3 origin, Vector3 destination, float inflation, int maxDepth, int divCt, int currentDepth, int layerMask, out List<Vector3> routePts)
    //{
    //    routePts = new List<Vector3>(){ origin };
    //    if (currentDepth > maxDepth)
    //    {
    //        Debug.Log("Max depth exceeded");
    //        return false;
    //    }
    //    RestrictionZoneBase obs;
    //    if (!ObstacleExists(origin, destination, layerMask, out obs))
    //    {
    //        routePts.Add(destination);
    //        return true;
    //    }
    //    currentDepth++;
    //    var paths = GetPathsAround(origin, destination, obs, inflation, divCt);
    //    if (paths == null)
    //    {
    //        Debug.Log("No paths around object could be found");
    //        return false;
    //    }
        
    //    var fixRoutes = new List<List<Vector3>>();
    //    //dbgPaths.AddRange(paths);
    //    foreach (var p in paths)
    //    {
    //        //find first edge that intersects an obstacle, if any
    //        //record the first obstacle
    //        //record an edge from start of that segment to the destination
    //        obs = null;
    //        bool foundObs = false;
    //        var rPts = new List<Vector3>(routePts);
    //        for (int v0 = 0; v0 < p.Count - 1; v0++)
    //        {
    //            int v1 = v0 + 1;
    //            if (ObstacleExists(p[v0], p[v1], layerMask, out obs))
    //            {
    //                foundObs = true;
    //                fixRoutes.Add(rPts);
    //                break;
    //            }
    //            rPts.Add(p[v1]);
    //        }
    //        if (!foundObs)
    //        {
    //            for (int i = 1; i < p.Count; i++)
    //            {
    //                routePts.Add(p[i]);
    //            }
    //            Debug.Log("Routed around obstacle");
    //            return true;
    //        }
    //    }
    //    foreach (var toFix in fixRoutes)
    //    {
    //        var rPts = new List<Vector3>();
    //        if (FindPath(toFix[toFix.Count - 1], destination, inflation, maxDepth, divCt, currentDepth, layerMask, out rPts))
    //        {
    //            routePts = new List<Vector3>(toFix);
    //            for (int i = 1; i < rPts.Count; i++)
    //            {
    //                routePts.Add(rPts[i]);
    //            }
    //            return true;
    //        }
    //    }

    //    Debug.Log("Path not found");
    //    return false;
    //}


    /// <summary>
    /// Routes around obstacle, if possible. Null on failure.
    /// </summary>
    private List<Vector3>[] GetPathsAround(Vector3 start, Vector3 end, List<Obstacle> obstacles, List<List<Obstacle>> obstacleGroups, float divDist, int layerMask, bool simpleConcavityEscape)
    {
        var allPts = new List<Vertex>();
        foreach (var o in obstacles)
        {
            foreach (var p in o.InflatedPts)
            {
                allPts.Add(new Vertex(p));
            }
        }
        allPts.Add(new Vertex(start));
        allPts.Add(new Vertex(end));
        var rawHullPts = JarvisMarchAlgorithm.GetConvexHull(allPts);
        var hullPts = new List<Vector3>();
        foreach (var p in rawHullPts)
        {
            hullPts.Add(p.position);
        }

        var haveI0 = GetPtIdx(hullPts, start, out int i0);
        var haveI1 = GetPtIdx(hullPts, end, out int i1);
        if (!haveI0 || !haveI1)
        {
            if (!haveI0)
            {
                if (!RouteVertexToHull(start, ref hullPts, obstacleGroups, divDist, layerMask, simpleConcavityEscape))
                {
                    Debug.Log("Could not route from point to hull.");
                    return null;
                }
            }
            if (!haveI1)
            {
                if (!RouteVertexToHull(end, ref hullPts, obstacleGroups, divDist, layerMask, simpleConcavityEscape))
                {
                    Debug.Log("Could not route from point to hull.");
                    return null;
                }
            }

            if (!GetPtIdx(hullPts, start, out i0) || !GetPtIdx(hullPts, end, out i1))
            {
                Debug.LogError("ERROR: failed to find start or end point in list of modified hull points.");
                return null;
            }
        }

        var ptsPos = new List<Vector3> { hullPts[i0] };
        var ptsNeg = new List<Vector3> { hullPts[i0] };
        var dest = hullPts[i1];
        bool foundPos = false;
        bool foundNeg = false;
        for (int i = 1; i <= hullPts.Count && (!foundPos || !foundNeg); i++)
        {
            if (!foundPos)
            {
                foundPos = BuildSinglePathAround(hullPts, ref ptsPos, i0, i, true, dest);
            }
            if (!foundNeg)
            {
                foundNeg = BuildSinglePathAround(hullPts, ref ptsNeg, i0, i, false, dest);
            }
        }
        //These conditions shouldn't happen because two paths around a convex hull should be possible. They are error conditions.
        if (!foundPos)
        {
            Debug.LogError("Failed to find path in positive direction around obstacle");
            return null;
        }
        if (!foundNeg)
        {
            Debug.LogError("Failed to find path in negative direction around obstacle");
            return null;
        }


        if (GetPathLength(ptsPos) < GetPathLength(ptsNeg))
        {
            return new[] { ptsPos, ptsNeg };
        }

        return new[] { ptsNeg, ptsPos };
    }

    /// <summary>
    /// Returns false if could not find a path from vertex to hull. Returns true and the point if so.
    /// </summary>
    private bool RouteVertexToHull(Vector3 v, ref List<Vector3> hull, List<List<Obstacle>> obstacleGroups, float divDist, int layerMask, bool simpleConcavityEscape)
    {
        List<Tuple<Vector3, float, bool, int>> goodPts = new List<Tuple<Vector3, float, bool, int>>();

        //first attempt to find closest point on segments, and if one works, call it the solution
        //modeled on solution for closest point on line found at: https://forum.unity.com/threads/math-problem.8114/#post-59715
        for (int i = 0; i < hull.Count; i++)
        {
            int j = i < hull.Count - 1 ? i + 1 : 0;
            var h0 = hull[i];
            var h1 = hull[j];
            var dir = (h1 - h0).normalized;
            var h0ToV = v - h0;
            float segmentLength = Vector3.Distance(h0, h1);
            float segmentParam = Vector3.Dot(dir, h0ToV);
            var closestPt = new Vector3();
            bool atEnd = false;
            if (segmentParam <= 0)
            {
                closestPt = h0;
                atEnd = true;
            }
            else if (segmentParam >= segmentLength)
            {
                closestPt = h1;
                atEnd = true;
            }
            else
            {
                var h0ToClosest = dir * segmentParam;
                closestPt = h0 + h0ToClosest;
            }

            if (!ObstacleExists(v, closestPt, obstacleGroups, layerMask, out _))
            {

                goodPts.Add(Tuple.Create(closestPt, Vector3.Distance(v, closestPt), atEnd, i));
                break;
            }
        }


        //if not, and if we are doing a more accurate path-finding, we can do this:
        if (!simpleConcavityEscape && goodPts.Count == 0)
        {
            for (int i = 0; i < hull.Count; i++)
            {
                int j = i < hull.Count - 1 ? i + 1 : 0;
                var h0 = hull[i];
                var h1 = hull[j];
                var dir = (h1 - h0).normalized;
                float segmentLength = Vector3.Distance(h0, h1);
                int divCt = (int) Math.Floor(segmentLength / divDist);
                float inc = segmentLength / divCt;
                for (int z = 0; z < divCt; z++)
                {
                    float d = z * inc;
                    var sample = h0 + dir * d;
                    if (!ObstacleExists(v, sample, obstacleGroups, layerMask, out _))
                    {
                        goodPts.Add(Tuple.Create(sample, Vector3.Distance(v, sample), z == 0, i));
                    }
                }
            }
        }

        if (goodPts.Count == 0)
        {
            Debug.Log("No good path found out of concave corner");
            return false;
        }
        goodPts = goodPts.OrderBy(p => p.Item2).ToList();
        var foundPt = goodPts[0].Item1;
        var foundIsHullPt = goodPts[0].Item3;
        var insertionIndex = goodPts[0].Item4;

        if (foundIsHullPt)
        {
            hull.Insert(insertionIndex + 1, v);
            hull.Insert(insertionIndex + 2, foundPt);
        }
        else
        {
            hull.Insert(insertionIndex + 1, foundPt);
            hull.Insert(insertionIndex + 2, v);
            hull.Insert(insertionIndex + 3, foundPt);
        }

        return true;
    }

    ///// <summary>
    ///// Routes around obstacle, if possible. Returns array ordered from shortest to longest path. Null on failure.
    ///// </summary>
    //private List<Vector3>[] GetPathsAround(Vector3 start, Vector3 end, RestrictionZoneBase obstacle, float inflation, int divCt)
    //{

    //    //var off1 = OffsetCrv(obstacle, inflation);
    //    //var off2 = OffsetCrv(obstacle, inflation * -1);
    //    //if (off1 == null && off2 == null)
    //    //{
    //    //    Print("Offset failed");
    //    //    return null;
    //    //}
    //    //double d1 = off1 != null ? off1.GetLength() : 0;
    //    //double d2 = off2 != null ? off2.GetLength() : 0;
    //    //Curve inflatedObs = d1 > d2 ? off1 : off2;

    //    var inflatedPts = obstacle.GetBoundaryPtsAtHeight(start.y, inflation);
    //    var allPts = new List<Vertex>();
    //    foreach (var p in inflatedPts)
    //    {
    //        allPts.Add(new Vertex(p));
    //    }
    //    allPts.Add(new Vertex(start));
    //    allPts.Add(new Vertex(end));

    //    //i guess we don't need to check containment...it's nice but not required.

    //    //if (inflatedObs.Contains(edge.From, Plane.WorldXY, 0.001) != PointContainment.Outside || inflatedObs.Contains(edge.To, Plane.WorldXY, 0.001) != PointContainment.Outside)
    //    //{
    //    //    Print("Skipping this convex hull because the start point is inside the obstacle in question");
    //    //    return null;
    //    //}
    //    //Point3d[] cPts;
    //   // inflatedObs.DivideByCount(divCt, true, out cPts);
    //    //List<Point3d> allPts = cPts.ToList();
    //    //allPts.Add(edge.From);
    //    //allPts.Add(edge.To);
    //    var rawHullPts = JarvisMarchAlgorithm.GetConvexHull(allPts);
    //    var hullPts = new List<Vector3>();
    //    foreach (var p in rawHullPts)
    //    {
    //        hullPts.Add(p.position);
    //    }
    //    int i0, i1;

    //    if (!GetPtIdx(hullPts, start, out i0) || !GetPtIdx(hullPts, end, out i1))
    //    {
    //        Debug.Log("Skipping this convex hull because the start point is likely inside the obstacle in question");
    //        return null;
    //    }
    //    List<Vector3> ptsPos = new List<Vector3> { hullPts[i0] };
    //    List<Vector3> ptsNeg = new List<Vector3> { hullPts[i0] };
    //    Vector3 dest = hullPts[i1];
    //    bool foundPos = false;
    //    bool foundNeg = false;
    //    for (int i = 1; i <= hullPts.Count && (!foundPos || !foundNeg); i++)
    //    {
    //        if (!foundPos)
    //        {
    //            foundPos = BuildSinglePathAround(hullPts, ref ptsPos, i0, i, true, dest);
    //        }
    //        if (!foundNeg)
    //        {
    //            foundNeg = BuildSinglePathAround(hullPts, ref ptsNeg, i0, i, false, dest);
    //        }
    //    }
    //    //These conditions shouldn't happen because two paths around a convex hull should be possible. They are error conditions.
    //    if (!foundPos)
    //    {
    //        Debug.LogError("Failed to find path in positive direction around obstacle");
    //        return null;
    //    }
    //    if (!foundNeg)
    //    {
    //        Debug.LogError("Failed to find path in negative direction around obstacle");
    //        return null;
    //    }

    //    if (GetPathLength(ptsPos) < GetPathLength(ptsNeg))
    //    {
    //        return new[] { ptsPos, ptsNeg };
    //    }

    //    return new [] { ptsNeg, ptsPos };
    //}

    /// <summary>
    /// Returns length of journey through all points in path.
    /// </summary>
    private float GetPathLength(List<Vector3> pts)
    {
        float length = 0;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            int j = i + 1;
            length += Vector3.Distance(pts[i], pts[j]);
        }

        return length;
    }

    /// <summary>
    /// Builds the path around the hull in one direction (positive or negative), returning true when destination point reached.
    /// </summary>
    private bool BuildSinglePathAround(List<Vector3> hullPts, ref List<Vector3> route, int startI, int currentStep, bool isPos, Vector3 destination)
    {
        int nextPtIdx;
        if (isPos)
        {
            nextPtIdx = startI + currentStep < hullPts.Count ? startI + currentStep : (startI + currentStep) - hullPts.Count;
        }
        else
        {
            nextPtIdx = startI - currentStep >= 0 ? startI - currentStep : hullPts.Count + (startI - currentStep);
        }

        Vector3 nextPt = hullPts[nextPtIdx];
        route.Add(nextPt);
        return (destination - nextPt).magnitude < 0.001;
    }

    /// <summary>
    /// Returns true if found point, and index of it.
    /// </summary>
    private bool GetPtIdx(List<Vector3> points, Vector3 p, out int index)
    {
        index = -1;
        for (int i = 0; i < points.Count; i++)
        {
            if (Vector3.Distance(points[i], p) < 0.001)
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    ///// <summary>
    ///// Returns true, if obstacle exists, and first obstacle
    ///// </summary>
    //private bool ObstacleExists(Vector3 from, Vector3 to, int layerMask, out RestrictionZoneBase obs)
    //{
    //    obs = null;
    //    var h = Physics.RaycastAll(from, to - from, Vector3.Distance(from, to), layerMask);
    //    if (h == null || h.Length == 0)
    //    {
    //        return false;
    //    }
    //    obs = GetClosestHitRestrictionZone(from, h)?.RestrictionZoneSpecs;
    //    return obs != null;
    //}

    /// <summary>
    /// Returns true, if obstacle exists, and first obstacle
    /// </summary>
    private bool ObstacleExists(Vector3 from, Vector3 to, List<List<Obstacle>> obstacleGroups, int layerMask, out List<Obstacle> obs)
    {
        obs = new List<Obstacle>();
        var h = Physics.RaycastAll(from, to - from, Vector3.Distance(from, to), layerMask);
        if (h == null || h.Length == 0)
        {
            return false;
        }
        var rZ = GetClosestHitRestrictionZone(from, h);
        if (rZ == null)
        {
            Debug.LogError("None of the hit objects is a restriction zone");
            return false;
        }

        foreach (var oG in obstacleGroups)
        {
            foreach (var o in oG)
            {
                if (o.ObjectId == rZ.Guid)
                {
                    obs = oG;
                    return true;
                }
            }
        }

        Debug.LogError("Couldn't find hit restriction zone in set of obstacle groups");
        return false;
    }

    public List<Vector3> FindPath(Vector3 origin, Vector3 destination, int angleIncrement, int layerMask)
    {
        // For pathfinding, omit drone colliders

        int head = 0, tail = 0;
        List<Vector3> visited = new List<Vector3>();
        List<int> from = new List<int>();
        List<float> distance = new List<float>();
        visited.Add(origin);
        from.Add(-1);
        distance.Add(0.0f);
        tail++;
        int iter = 0;
        int maxIter = 1000;

        while (iter <= maxIter && head < visited.Count && Physics.Raycast(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask) && head <= tail)
        {
            var h = Physics.RaycastAll(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask);
            RaycastHit currentHitObject = ViewportUtils.GetClosestHit(visited[head], h);
            Vector3 lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 180; i += angleIncrement)
            {
                //Vector3 currentVector = destination - visited[head];


                //to go around an object, you have two choices: left or right
                //we choose one direction, based on least deviation from current heading
                //but we need to know both in order to evaluate.


                RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination), layerMask);
                if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                {
                    Vector3 newWaypoint = RotateAround(lastHit, visited[head], (float)angleIncrement);
                    visited.Add(newWaypoint);
                    from.Add(head);
                    distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                    tail++;
                    if (hit.Length > 0)
                    {
                        Debug.Log($"Routed around object {hit[0].transform.gameObject.name}");
                    }
                    break;
                }
                lastHit = hit[0].point;
                
            }

            // Do the same thing in the opposite direction
            lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 180; i += angleIncrement)
            {
                //Vector3 currentVector = destination - visited[head];
                RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, -i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination), layerMask);
                if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                {
                    Vector3 newWaypoint = RotateAround(lastHit, visited[head], -(float)angleIncrement);
                    visited.Add(newWaypoint);
                    from.Add(head);
                    distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                    tail++;
                    break;
                }
                lastHit = hit[0].point;
            }

            head++;
            iter++;
        }
        if (iter == maxIter)
        {
            Debug.LogError("Error finding route");
        }

        // Do the same thing in the opposite direction
        List<Vector3> path = new List<Vector3>();
        if (head < tail) // found a path so backtrack
        {
            if (head > 0) path = Backtrack(visited, from, distance, head);
            path.Add(destination);
        }
        else // cannot find a path - just fly straight
        {
            path.Add(destination);
        }

        if (path.Count == 0) path.Add(destination);
        return path;
    }

    List<Vector3> Backtrack(List<Vector3> visited, List<int> from, List<float> distance, int head)
    {
        List<Vector3> result = new List<Vector3>();
        int pointer = head;
        do
        {
            result.Add(visited[pointer]);
            pointer = from[pointer];
        } while (pointer >= 0 && from[pointer] != -1);
        result.Reverse();
        return result;
    }

    /// <summary>
    /// Allows user to toggle route visualization at runtime.
    /// </summary>
    public void ToggleRouteVisualization(bool toggle)
    {
        _networkVisible = toggle;
        if (_networkVisible)
        {
            foreach (GameObject gO in networkLines)
            {
                LineRenderer lr = gO.GetComponent<LineRenderer>();
                lr.enabled = true;
            }
        }
        else
        {
            foreach (GameObject gO in networkLines)
            {
                LineRenderer lr = gO.GetComponent<LineRenderer>();
                lr.enabled = false;
            }
        }

    }

    public void UpdateNetworkLineWidths()
    {
        foreach (var nL in networkLines)
        {
            var r = nL.GetComponent<LineRenderer>();
            if (r == null)
            {
                continue;
            }

            r.startWidth = CalcLineWidth();
            r.endWidth = r.startWidth;
        }
    }

    private float CalcLineWidth()
    {
        var hP = _mainCamera.transform.position.y / _camStartHeight;
        return UISettings.REGIONVIEW_NETWORK_WIDTH_FACTOR * hP;
    }

    /// <summary>
    /// Builds visuals for network
    /// </summary>
    private void VisualizeNetwork()
    {
        if (networkLines.Count > 0)
        {
            for (int i = 0; i < networkLines.Count; i++)
            {
                networkLines[i].Destroy();
            }
            foreach (var c in routeLineObject.Keys)
            {
                c.OnCongestionLevelChange -= CongestionLevelChangeHandler;
            }
        }
        networkLines.Clear();
        routeLineObject.Clear();
        float width = CalcLineWidth();
        foreach (Corridor c in DroneNetwork.corridors)
        {
            string name = "Corridor_" + c.origin.name + "_" + c.destination.name;

            List<Vector3> wayPointsList = new List<Vector3>(c.wayPoints.ToArray());
            Vector3[] wayPointArray = new Vector3[wayPointsList.Count];
            for (int i = 0; i < wayPointsList.Count; i++)
            {
                wayPointArray[i] = new Vector3(wayPointsList[i].x, c.elevation, wayPointsList[i].z);
            }
            var lineRenderer = InstantiationUtils.MakePolyline(wayPointArray, Resources.Load<Material>("Materials/Route"), width, false, name);
            networkLines.Add(lineRenderer.gameObject);
            routeLineObject.Add(c, lineRenderer.gameObject);
            c.OnCongestionLevelChange += CongestionLevelChangeHandler;
        }

        ToggleRouteVisualization(_networkVisible);
    }

    /// <summary>
    /// Function to be called when congestion level changes in a corridor.
    /// </summary>
    private void CongestionLevelChangeHandler(Corridor c, int congestionLevel)
    {
        GameObject line = routeLineObject[c];
        LineRenderer lr = line.GetComponent<LineRenderer>();
        if (congestionLevel == 1) lr.material = Resources.Load<Material>("Materials/Route_Low_Congestion");
        else if (congestionLevel == 2) lr.material = Resources.Load<Material>("Materials/Route_Medium_Congestion");
        else if (congestionLevel == 3) lr.material = Resources.Load<Material>("Materials/Route_High_Congestion");
        else lr.material = Resources.Load<Material>("Materials/Route");

        SimulationAnalyzer sa = GetComponent<SimulationAnalyzer>();
        if (congestionLevel > 0 && !sa.congestedCorridors.Contains(c)) sa.congestedCorridors.Add(c);
        else if (congestionLevel == 0 && sa.congestedCorridors.Contains(c)) sa.congestedCorridors.Remove(c);

    }
    #endregion
}

