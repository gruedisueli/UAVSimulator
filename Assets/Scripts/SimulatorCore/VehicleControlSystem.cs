using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;



using Assets.Scripts.SimulatorCore;
using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;
using Assets.Scripts.UI;
using Assets.Scripts;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI.Tools;

using DelaunatorSharp;
using Random = System.Random;


/// <summary>
/// Sort of a main control system for the entire fleet of drones in the simulation.
/// </summary>
public class VehicleControlSystem : MonoBehaviour
{
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
            GenerateNetwork();
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

                    var standBy = parking.GetComponent<ParkingControl>().parkingInfo.StandbyPosition;
                    var pt0 = new Vector3(parking.transform.position.x, EnvironManager.Instance.Environ.SimSettings.LowAltitudeFlightElevation_M, parking.transform.position.z);
                    var generatedPoints = FindPath(pt0, destination, 5, 1 << 9 | 1 << 8);
                    generatedPoints.Insert(0, pt0);
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
            landings.Add(sDP.gameObject);
        }
        List<int> indices = new List<int>();
        List<GameObject> destinationList = new List<GameObject>();
        int value = 0, destinationCount = UnityEngine.Random.Range(1, sceneManager.DronePorts.Values.Count + 1);
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
    /// Generates data structure for network
    /// </summary>
    private void GenerateNetwork()
    {
        DroneNetwork = new Assets.Scripts.DataStructure.Network();
        List<GameObject> points = new List<GameObject>();
        foreach (SceneDronePort sdp in sceneManager.DronePorts.Values) points.Add(sdp.gameObject);
        foreach (SceneParkingStructure sps in sceneManager.ParkingStructures.Values)
        {
            if (!sps.ParkingStructureSpecs.Type.Contains("LowAltitude")) points.Add(sps.gameObject);
        }

        if (points.Count < 3) return;

        IPoint[] vertices = GetVertices(points);
        Delaunator delaunay = new Delaunator(vertices);
        DroneNetwork.vertices = points;
        var utmElev = EnvironManager.Instance.Environ.SimSettings.CorridorFlightElevation_M;
        var utmSep = EnvironManager.Instance.Environ.SimSettings.CorridorSeparationDistance_M;
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

                var wayPoints = FindPath(corridor_from_to_start, corridor_from_to_end, 5, 1 << 9 | 1 << 8);
                wayPoints.Insert(0, corridor_from_to_start);
                corridor_from_to.wayPoints = new Queue<Vector3>(wayPoints);

                wayPoints = FindPath(corridor_to_from_start, corridor_to_from_end, 5, 1 << 9 | 1 << 8);
                wayPoints.Insert(0, corridor_to_from_start);
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

    RaycastHit GetClosestHit(Vector3 current, RaycastHit[] hit)
    {
        float minDist = Mathf.Infinity;
        RaycastHit minHit = hit[0];
        foreach (RaycastHit h in hit)
        {
            if (Vector3.Distance(current, h.point) < minDist)
            {
                minHit = h;
                minDist = Vector3.Distance(current, h.point);
            }
        }
        return minHit;
    }

    Vector3 RotateAround(Vector3 point, Vector3 pivot, float angle)
    {
        Vector3 newPoint = point - pivot;
        newPoint = Quaternion.Euler(0, angle, 0) * newPoint;
        newPoint = newPoint + pivot;
        return newPoint;
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
            RaycastHit currentHitObject = GetClosestHit(visited[head], h);
            Vector3 lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 85; i += angleIncrement)
            {
                Vector3 currentVector = destination - visited[head];
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
                else
                {
                    lastHit = hit[0].point;
                }
            }

            // Do the same thing in the opposite direction
            lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 85; i += angleIncrement)
            {
                Vector3 currentVector = destination - visited[head];
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
                else
                {
                    lastHit = hit[0].point;
                }
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

