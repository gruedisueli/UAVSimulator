using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Xml.Linq;
using System.IO;


using Assets.Scripts.Vehicle_Control;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;
using Assets.Scripts.UI;
using Assets.Scripts;
using Assets.Scripts.UI.EventArgs;
using DelaunatorSharp;


public class VehicleControlSystem : MonoBehaviour
{

    const float AGL_700_METERS = 213.36f;
    const float AGL_1200_METERS = 365.76f;
    public float MIN_DRONE_RANGE;
    const float UTM_ELEVATION = 152.4f; // 500 AGL ft
    const float SEPARATION = 25.0f;


    #region UI Variables

    public SimulationParam simulationParam;
    private Dictionary<string, float> statistics;

    
    #endregion

    #region Vehicle Info
    private Dictionary<GameObject, string> activeVehicles;
    #endregion



    #region Private Variables
    private Color translucentRed;
    private float watch;
    
    private bool vehicleInstantiated;

    // Visualization related params
    public bool playing;
    public bool noiseVisualization;
    public bool privacyVisualization;
    public bool routeVisualization;
    public bool landingCorridorVisualization;
    public bool demographicVisualization;
    public bool trailVisualization;

    // INTEGRATION TO-DO: Replace root with the path that it receives;
    private string root = "runtime\\";
    private string asset_root = "Assets\\";

    // private SceneManagerBase sceneManager;
    private CityViewManager sceneManager;
    public List<GameObject> vehicles;
    public List<GameObject> movingVehicles;

    // Background drone related params
    public List<GameObject> backgroundDrones;
    public int backgroundDroneCount = 100;
    public float lowerElevationBound = 100;
    public float upperElevationBound = 135;
    public float[][] cityBounds;

    public bool networkGenerated;
    public List<GameObject> parkingCollection;
    public List<GameObject> landingCollection;
    public Dictionary<GameObject, GameObject> parkingLandingMapping;
    public Dictionary<GameObject, List<GameObject>> routes;
    public Assets.Scripts.DataStructure.Network network;
    public List<GameObject> networkLines;

    public float speedMultiplier = 1.0f;

    public GameObject TypeAPrefab;
    #endregion



    private SignalSystem signalSystem;
    private string current_runtime;
    public event EventHandler<DroneInstantiationArgs> OnDroneInstantiated;

    // Start is called before the first frame update
    void Start()
    {
        playing = false;
        vehicleInstantiated = false;
        networkGenerated = false;
        translucentRed = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.50f);
        // 1. instantiate vehicles in parking spots
        // 2. generate call signals
        MIN_DRONE_RANGE = 99999.0f;
        string current_runtime = "42o3601_71o0589"; // INTEGRATION TO-DO: Get current runtime name from UI side to find out which folder to refer to
                                                    // Current Placeholder = Lat_Long of Boston

        backgroundDrones = new List<GameObject>();
        simulationParam = ReadSimulationParams(current_runtime);
        // TO-DO: Add clause that checks if we are doing simulation in RegionView
        sceneManager = GameObject.Find("FOA").GetComponent<CityViewManager>();
    
        movingVehicles = new List<GameObject>();
        
        

        speedMultiplier = 2.0f;

        watch = 0.0f;

        var eM = EnvironManager.Instance;
        var city = eM.GetCurrentCity();
        cityBounds = UnitUtils.GetCityExtents(city.CityStats);
        InstantiateBackgroundDrones();
    }

    // Update is called once per frames
    void Update()
    {
        if (playing)
        {
            watch += Time.deltaTime;
            if (watch > simulationParam.callGenerationInterval) /*&& movingVehicles.Count < simulationParam.maxInFlightVehicles)*/
            {
                watch = 0.0f;
                GenerateRandomCalls();
            }
        }
    }


    #region Methods

    /// <summary>
    /// Called (from scene manager) whenever we push the button that plays/pauses simulation. If currently playing, pauses simulation, else plays simulation.
    /// </summary>
    public void PlayPause()
    {
        playing = !playing;
        var eM = EnvironManager.Instance;
        var city = eM.GetCurrentCity();
        cityBounds = UnitUtils.GetCityExtents(city.CityStats);
        if ( playing )
        {
            foreach (GameObject gO in GameObject.FindObjectsOfType<GameObject>())
            {
                if(gO.name.Contains("Building"))
                {
                    gO.AddComponent<BuildingNoise>();
                    gO.layer = 9;
                    gO.tag = "Building";
                }
            }

            if (!vehicleInstantiated)
            {
                InstantiateVehicles();
                vehicleInstantiated = true;
            }
            if(!networkGenerated)
            {
                GenerateNetwork();
                VisualizeNetwork(network);
                networkGenerated = true;
            }
        }
    }

    

    public void GenerateRandomCalls()
    {
        int call_type = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 3.0f));
        string call_type_string = call_type <= 3 ? "corridor" : "low-altitude";

        // strategicDeconfliction == "none"
        if (simulationParam.strategicDeconfliction.Equals("none"))
        {
            if (call_type_string.Equals("corridor"))
            {
                List<GameObject> destinations_list = GetNewRandomDestinations();
                GameObject parking = GetNearestAvailableParking(destinations_list[0]);
                Queue<GameObject> destinations = Route(parking, destinations_list);
                // From Here - Replace the part that returns waypoints - consider different ends
                //           - Visualize corridor
                //           - Visualize landing paths
                //           - Hook-up toggles
                if (parking == null)
                {
                    Debug.Log("No available vehicle");
                    return;
                }
                // TO-DO: Augment vehicle type
                GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);
                if (vehicle == null)
                {
                    Debug.Log("No available vehicle");
                    return;
                }
                if (parking.GetComponent<ParkingControl>().queue.Count < 3) CallVehicle(vehicle, parking.GetComponent<ParkingControl>(), destinations);
            }
            else // call_type_string == "low-altitude"
            {
                int withAAO = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 1.0f));
                string withAAOString = withAAO == 0 ? "AAO" : "None";

                if (withAAOString.Equals("AAO")) // generating calls with AAO
                {
                    // Create random polygon
                    Polygon p = new Polygon(Mathf.RoundToInt(UnityEngine.Random.Range(3, 10)));

                    // Generate random center within the boundary of the maps - Integration TO-DO: Get the boundary coordinates from UI
                    Bounds boston_bd = GameObject.Find("Buildings").GetComponentInChildren<MeshRenderer>().bounds;
                    Vector3 polygonCenter = GetRandomAAO(boston_bd.min, boston_bd.max);
                    p.Move(polygonCenter);

                    // TO-DO: Augment vehicle type (make it find the right type) and make it possible to have more than one drone in operation per AAO
                    GameObject parking = GetNearestAvailableParking(polygonCenter);
                    GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);

                    // Generate 
                    Mesh m = p.CreateExtrusion(simulationParam.lowAltitudeBoundary);
                    var AAO = new GameObject("AAO_" + vehicle.name);
                    AAO.AddComponent<MeshFilter>().mesh = m;
                    MeshRenderer mr = AAO.AddComponent<MeshRenderer>();
                    mr.material.color = translucentRed;
                    mr.material.SetOverrideTag("RenderType", "Transparent");
                    mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mr.material.SetInt("_ZWrite", 0);
                    mr.material.DisableKeyword("_ALPHATEST_ON");
                    mr.material.EnableKeyword("_ALPHABLEND_ON");
                    mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mr.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    AAO.layer = 8;
                    AAOControl aaoCon = AAO.AddComponent<AAOControl>();
                    aaoCon.AddVehicle(vehicle);
                    
                    // iterate below for each vehicle that are operating in this AAO
                    List<Vector3> generatedPoints = p.GeneratePointsinExtrusion(aaoCon.GetVehicleCount(), simulationParam.lowAltitudeBoundary);
                    Queue<GameObject> destinations = new Queue<GameObject>();
                    int count = 0;
                    foreach (Vector3 gp in generatedPoints)
                    {
                        GameObject newPointOfOperation = new GameObject(AAO.name + "_" + count.ToString());
                        newPointOfOperation.transform.position = gp;
                        newPointOfOperation.transform.SetParent(AAO.transform);
                        count++;
                        destinations.Enqueue(newPointOfOperation);
                    }
                    Vehicle vehicleInfo = vehicle.GetComponent<Vehicle>();
                    vehicleInfo.isUTM = true;
                    
                    CallVehicle(vehicle, parking.GetComponent<ParkingControl>(), destinations);
                }
            }
        }
        else
        {


        }

    }

    public Vector3 GetRandomAAO( Vector3 minPoint, Vector3 maxPoint )
    {
        return new Vector3(UnityEngine.Random.Range(minPoint.x, maxPoint.x), 0, UnityEngine.Random.Range(minPoint.z, maxPoint.z));
    }

    /*
        public List<Vector3> FindPath(Vector3 origin, Vector3 destination, int angleIncrement)
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

            ///////////////////////////////// TACKLE HERE /////////////////////////////////
            while (head < tail && Physics.Raycast(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination)) )
            {
                RaycastHit currentHitObject = Physics.RaycastAll(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination))[0];
                Vector3 lastHit = currentHitObject.point;
            ///////////////////////////////// TACKLE HERE /////////////////////////////////
                for (int i = angleIncrement; i <= 85; i += angleIncrement)
                {
                    Vector3 currentVector = destination - visited[head];
                    RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination));
                    if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                    {
                        Vector3 newWaypoint = RotateAround(lastHit, visited[head], (float)angleIncrement);
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

                // Do the same thing in the opposite direction
                lastHit = currentHitObject.point;
                for (int i = angleIncrement; i <= 85; i += angleIncrement)
                {
                    Vector3 currentVector = destination - visited[head];
                    RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, -i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination));
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
            }
            // Do the same thing in the opposite direction
            List<Vector3> path = new List<Vector3>();
            if (head < tail) // found a path so backtrack
            {
                if(head > 0) path = Backtrack(visited, from, distance, head);
                path.Add(destination);
            }
            else // cannot find a path - just fly straight
            {
                //path.Add(origin);
                path.Add(destination);
            }
            return path;
        }*/
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
    public List<Vector3> FindPath(Vector3 origin, Vector3 destination, int angleIncrement)
    {
        // For pathfinding, omit drone colliders
        int layerMask = 1 << 9;
        int head = 0, tail = 0;
        List<Vector3> visited = new List<Vector3>();
        List<int> from = new List<int>();
        List<float> distance = new List<float>();
        visited.Add(origin);
        from.Add(-1);
        distance.Add(0.0f);
        tail++;

        while (Physics.Raycast(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask) && head <= tail)
        {
            RaycastHit currentHitObject = GetClosestHit(visited[head], Physics.RaycastAll(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask));
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
            //path.Add(origin);
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
        //if (pointer != -1) result.Add(visited[pointer]);
        result.Reverse();
        return result;
    }
    Vector3 RotateAround(Vector3 point, Vector3 pivot, float angle)
    {
        Vector3 newPoint = point - pivot;
        newPoint = Quaternion.Euler(0, angle, 0) * newPoint;
        newPoint = newPoint + pivot;
        return newPoint;
    }
    private void CallVehicle(GameObject vehicle, ParkingControl parking, Queue<GameObject> destinations)
    {
        Vehicle vehicleInfo = vehicle.GetComponent<Vehicle>();
        vehicleInfo.destination = destinations;
        vehicleInfo.state = "takeoff_requested";    
        parking.queue.Enqueue(vehicle);
    }

    public float GetElevation(GameObject origin, GameObject destination)
    {
        // TO-DO: Assign elevation according to the simulation rules
        // Now: All 100m to test obstacle avoidance
        if (origin.transform.position.y > 100.0f) return origin.transform.position.y + 50.0f;
        return 152.0f;
    }
    private void InstantiateVehicles()
    {
        string path = asset_root + "Resources\\Drones\\";
        var files = Directory.GetFiles(path, "*.JSON");
        Dictionary<string, VehicleSpec> vehicleSpecs = new Dictionary<string, VehicleSpec>();
        List<string> vehicleTypes = new List<string>();
        foreach (var filename in files)
        {
            string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
            VehicleSpec vs = JsonUtility.FromJson<VehicleSpec>(json);
            vehicleTypes.Add(vs.type);
            vehicleSpecs.Add(vs.type, vs);
            if (vs.range < MIN_DRONE_RANGE) MIN_DRONE_RANGE = vs.range;
        }
        int parkingCapacity = sceneManager.GetParkingCapacity();
        int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity-10, parkingCapacity);
        string drone_path = "Drones/";

        // Populate vehiclesToInstantiate number of drones in existing parking structures
        for (int i = 0; i < vehiclesToInstantiate; i++)
        {
            // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated
            foreach (var key in sceneManager.ParkingStructures.Keys)
            {
                var sPS = sceneManager.ParkingStructures[key];
                ParkingControl pC = sPS.ParkingCtrl;
                if(pC.parkingInfo.Parked.Count == 0 ) pC.parkingInfo.RemainingSpots = pC.parkingInfo.ParkingSpots.Count - pC.parkingInfo.Parked.Count;
                if (pC.parkingInfo.RemainingSpots > 0)
                {
                    int vehicleTypeID = UnityEngine.Random.Range(0, vehicleTypes.Count);
                    var newDrone = Resources.Load<GameObject>(drone_path + vehicleTypes[vehicleTypeID]);
                    var type = vehicleTypes[vehicleTypeID];
                    var emptySpot = pC.parkingInfo.GetEmptySpot();
                    var translatedSpot = pC.parkingInfo.TranslateParkingSpot(emptySpot);

                    // instantiate the vehicle at emptySpot
                    var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                    clone.name = "UAV_" + i.ToString();
                    clone.tag = "Vehicle";
                    clone.layer = 10;
                    clone.AddComponent<VehicleNoise>();
                    TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                    tr.material = Resources.Load<Material>("Materials/TrailCorridorDrones");
                    tr.time = Mathf.Infinity;
                    tr.enabled = false;
                    UnityEngine.Object.Destroy(newDrone);

                    // Fill in vehivle spec
                    Vehicle v = clone.AddComponent<Vehicle>();
                    v.SetVehicleInfo(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise);
                    v.currentPoint = sPS.gameObject;
                    vehicles.Add(clone);
                    OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
                    // Update parking management info
                    pC.parkingInfo.ParkAt(emptySpot, clone);

                    break;
                }
            }
        }
    }

    private void InstantiateBackgroundDrones()
    {
        string path = asset_root + "Resources\\Drones\\";
        var files = Directory.GetFiles(path, "*.JSON");
        Dictionary<string, VehicleSpec> vehicleSpecs = new Dictionary<string, VehicleSpec>();
        List<string> vehicleTypes = new List<string>();
        foreach (var filename in files)
        {
            string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
            VehicleSpec vs = JsonUtility.FromJson<VehicleSpec>(json);
            vehicleTypes.Add(vs.type);
            vehicleSpecs.Add(vs.type, vs);
            if (vs.range < MIN_DRONE_RANGE) MIN_DRONE_RANGE = vs.range;
        }
        
        int vehiclesToInstantiate = backgroundDroneCount;
        string drone_path = "Drones/";
        Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
        // Populate vehiclesToInstantiate number of drones in existing parking structures
        for (int i = 0; i < vehiclesToInstantiate; i++)
        {
            // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated
            
            int vehicleTypeID = UnityEngine.Random.Range(0, vehicleTypes.Count);
            var newDrone = Resources.Load<GameObject>(drone_path + vehicleTypes[vehicleTypeID]);
            var type = vehicleTypes[vehicleTypeID];

            float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
            Vector3 instantiationSpot = GetRandomPointXZ(y);
            // instantiate the vehicle at emptySpot
            var clone = Instantiate(newDrone, instantiationSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            clone.name = "UAV_BACKGROUND_" + i.ToString();
            clone.tag = "Vehicle";
            clone.layer = 10;
            //clone.AddComponent<VehicleNoise>();
            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
            tr.startColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.endColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.material = backgroundTrail;
            tr.time = 60.0f;
            UnityEngine.Object.Destroy(newDrone);

            // Fill in vehivle spec
            Vehicle v = clone.AddComponent<Vehicle>();
            v.SetVehicleInfo(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise);
            //v.currentPoint = v.gameObject.transform.posit;
            v.isBackgroundDrone = true;
            Vector3 firstDestination = GetRandomPointXZ(y);
            v.wayPoints = FindPath(instantiationSpot, firstDestination, 5);
            v.wayPointsQueue = new Queue<Vector3>();
            foreach ( Vector3 p in v.wayPoints)
            {
                v.wayPointsQueue.Enqueue(p);
            }
            v.currentTargetPosition = v.wayPointsQueue.Dequeue();
            backgroundDrones.Add(clone);

            // Update parking management info

            
            
        }
    }

    public IPoint[] GetVertices(List<GameObject> points)
    {
        List<IPoint> pts = new List<IPoint>();
        foreach(GameObject gO in points)
        {
            Vector3 position = gO.transform.position;
            pts.Add(new Point((double)position.x, (double)position.z));
        }
        return pts.ToArray();
    }

    
    public int nextHalfEdge(int e)
    {
        return (e % 3 == 2) ? e - 2 : e + 1;
    }
    public void GenerateNetwork()
    {
        List<GameObject> points = new List<GameObject>();
        foreach(SceneDronePort sdp in sceneManager.DronePorts.Values) points.Add(sdp.gameObject);
        foreach(SceneParkingStructure sps in sceneManager.ParkingStructures.Values) points.Add(sps.gameObject);

        IPoint[] vertices = GetVertices(points);
        Delaunator delaunay = new Delaunator(vertices);
        network.vertices = points;
        for ( int i = 0; i < delaunay.Triangles.Length; i++ )
        {
            if( i > delaunay.Halfedges[i])
            {
                GameObject from = points[delaunay.Triangles[i]];
                GameObject to = points[delaunay.Triangles[nextHalfEdge(i)]];
                Corridor corridor_from_to = new Corridor(from, to, UTM_ELEVATION);
                Vector3 corridor_from_to_start = new Vector3(from.transform.position.x, UTM_ELEVATION, from.transform.position.z);
                Vector3 corridor_from_to_end = new Vector3(to.transform.position.x, UTM_ELEVATION, to.transform.position.z);
                corridor_from_to_start.y = UTM_ELEVATION;
                corridor_from_to_end.y = UTM_ELEVATION;

                Corridor corridor_to_from = new Corridor(to, from, UTM_ELEVATION + SEPARATION);
                Vector3 corridor_to_from_start = new Vector3(to.transform.position.x, UTM_ELEVATION + SEPARATION, to.transform.position.z);
                Vector3 corridor_to_from_end = new Vector3(from.transform.position.x, UTM_ELEVATION + SEPARATION, from.transform.position.z);

                corridor_from_to.wayPoints = new Queue<Vector3>(FindPath(corridor_from_to_start, corridor_from_to_end, 5).ToArray());
                corridor_to_from.wayPoints = new Queue<Vector3>(FindPath(corridor_to_from_start, corridor_to_from_end, 5).ToArray());
                
                network.corridors.Add(corridor_from_to);
                network.corridors.Add(corridor_to_from);
                if (!network.inEdges.ContainsKey(from)) network.inEdges.Add(from, new List<Corridor>());
                if (!network.outEdges.ContainsKey(from)) network.outEdges.Add(from, new List<Corridor>());
                if (!network.inEdges.ContainsKey(to)) network.inEdges.Add(to, new List<Corridor>());
                if (!network.outEdges.ContainsKey(to)) network.outEdges.Add(to, new List<Corridor>());

                network.outEdges[from].Add(corridor_from_to);
                network.inEdges[from].Add(corridor_to_from);
                network.outEdges[to].Add(corridor_to_from);
                network.inEdges[to].Add(corridor_from_to);
            }
        }

    }
    public Vector3 GetRandomPointXZ(float y)
    {
        
        float x = UnityEngine.Random.Range(cityBounds[0][0], cityBounds[0][1]);
        float z = UnityEngine.Random.Range(cityBounds[1][0], cityBounds[1][1]);
        return new Vector3(x, y, z);
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
            if (sPS.ParkingStructureSpecs.RemainingSpots > 0)
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
        sceneManager.ParkingStructures[nearestGuid].ParkingStructureSpecs.Reserve(v);
        return nearest;
    }
    private SimulationParam ReadSimulationParams(string runtime_name)
    {
        string path = root + runtime_name + "\\" + "simulation.json";
        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        SimulationParam sp = JsonUtility.FromJson<SimulationParam>(json);

        return sp;
    }
    public GameObject GetAvailableVehicleinParkingStrcuture(GameObject parkingStructure)
    {
        for ( int i = 0; i < parkingStructure.GetComponent<ParkingControl>().parkingInfo.VehicleAt.Keys.Count; i++)
        {
            GameObject vehicle_i = parkingStructure.GetComponent<ParkingControl>().parkingInfo.VehicleAt.Keys.ElementAt<GameObject>(i);
            if (vehicle_i.GetComponent<Vehicle>().state == "parked") return vehicle_i;
        }
        return null;
    }

    public Queue<GameObject> Route (GameObject origin, List<GameObject> destinations)
    {
        Queue<GameObject> routed_destinations = new Queue<GameObject>();
        destinations.Insert(0, origin);
        for ( int i = 0; i < destinations.Count - 1; i++ )
        {
            foreach(GameObject gO in Dijkstra(destinations[i], destinations[i+1], MIN_DRONE_RANGE))
            {
                routed_destinations.Enqueue(gO);
            }
        }
        routed_destinations.Dequeue();
        return routed_destinations;
    }



    // assumes a vehicle gets fully charged at each stop
    List<GameObject> Dijkstra(GameObject origin, GameObject destination, float vehicleRange)
    {
        Dictionary<GameObject, float> distanceTo = new Dictionary<GameObject, float>();
        Dictionary<GameObject, List<GameObject>> pathTo = new Dictionary<GameObject, List<GameObject>>();
        Queue<GameObject> queue = new Queue<GameObject>();

        distanceTo.Add(origin, 0.0f);
        pathTo.Add(origin, new List<GameObject>());
        pathTo[origin].Add(origin);

        queue.Enqueue(origin);
        do
        {
            GameObject currentNode = queue.Dequeue();
            Vector3 currentPoint = currentNode.transform.position;
            foreach (Corridor outEdge in network.outEdges[currentNode])
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
                }
            }
        } while (queue.Count > 0);
        return pathTo[destination];
    }


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
        /*
        List<GameObject> shortestPath = new List<GameObject>();
        //Debug.Log("Origin: " + destinationList[0].name + " - " + "Destination: " + destinationList[1].name);
        string routedResult = null;
        shortestPath = Dijkstra(destinationList[0], destinationList[1], range);
        foreach (GameObject g in shortestPath)
        {
            routedResult += g.name;
            routedResult += " ";
        }
        //Debug.Log("Routed: " + routedResult);

        Queue<GameObject> routed = new Queue<GameObject>();
        foreach (GameObject p in shortestPath) routed.Enqueue(p);

        return routed;*/
    }
    /*
    public Queue<GameObject> GetNewRandomDestinations ()
    {
        Queue<GameObject> newDestinations = new Queue<GameObject>();
        int count = Mathf.RoundToInt(Random.Range(1.0f, 5.0f));
        List<GameObject> landings = new List<GameObject>(landingCollection);

        for (int i = 0; i < landings.Count; i++)
        {
            GameObject temp = landings[i];
            int randomIndex = Random.Range(i, landings.Count);
            landings[i] = landings[randomIndex];
            landings[randomIndex] = temp;
        }

        int p = 0;
        for (int i = 0; i < landings.Count; i++)
        {
            
            if ( landings[i].GetComponent<Parking>() == null )
            {
                p++;
                newDestinations.Enqueue(landings[i]);
            }
            if (p == count) break;
        }
        return newDestinations;
    }*/
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
            if (sPS.ParkingCtrl.parkingInfo.VehicleAt.Keys.Count > 0)
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

    public GameObject GetNearestAvailableParking(Vector3 AAOCenter)
    {
        GameObject closestParking = null;

        float minDistance = Mathf.Infinity;
        // For all parking structures
        foreach (var sPS in sceneManager.ParkingStructures.Values)
        {
            var gO = sPS.gameObject;
            // find the nearest one with parked vehicles
            if (sPS.ParkingCtrl.parkingInfo.VehicleAt.Keys.Count > 0)
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

    public bool Register(Vehicle v)
    {
        return true;
        /*
        if (!activeVehicles.ContainsKey(v.id))
        {
            activeVehicles.Add(v.id, v);
            return true;
        }
        else
        {
            return false;
        }*/
    }
    public bool UpdateVehicleStatus(Vehicle v)
    {
        if (activeVehicles.ContainsKey(v.gameObject))
        {
            //Debug.Log(v.id + " " + v.status);
            activeVehicles[v.gameObject] = v.state;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateVehicleCount(int count)
    {

    }

    public void ToggleNoiseVisualization ( bool toggle )
    {
        noiseVisualization = toggle;
    }
    public void ToggleTrailVisualization(bool toggle)
    {
        trailVisualization = toggle;
    }
    public void TogglePrivacyVisualization(bool toggle)
    {
        privacyVisualization = toggle;
    }
    public void ToggleRouteVisualization(bool toggle)
    {
        routeVisualization = toggle;
        if (routeVisualization)
        {
            if(networkGenerated)
            {
                foreach (GameObject gO in networkLines)
                {
                    LineRenderer lr = gO.GetComponent<LineRenderer>();
                    lr.enabled = true;
                }
            }
        }
        else
        {
            if (networkGenerated)
            {
                foreach (GameObject gO in networkLines)
                {
                    LineRenderer lr = gO.GetComponent<LineRenderer>();
                    lr.enabled = false;
                }
            }

        }

    }
    public void ToggleLandingCorridorVisualization(bool toggle)
    {
        landingCorridorVisualization = toggle;
    }
    public void ToggleDemographicVisualization(bool toggle)
    {
        demographicVisualization = toggle;
    }

    public void VisualizeNetwork ( Assets.Scripts.DataStructure.Network network )
    {
        networkLines = new List<GameObject>();
        foreach(Corridor c in network.corridors)
        {
            GameObject line = new GameObject();
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.positionCount = c.wayPoints.Count + 1;
            List<Vector3> wayPointsList = new List<Vector3>(c.wayPoints.ToArray());
            wayPointsList.Insert(0, c.origin.transform.position);
            Vector3[] wayPointArray = new Vector3[wayPointsList.Count];
            for(int i = 0; i < wayPointsList.Count; i++)
            {
                wayPointArray[i] = new Vector3(wayPointsList[i].x, c.elevation, wayPointsList[i].z);
            }
            lineRenderer.SetPositions(wayPointArray);
            lineRenderer.material = Resources.Load<Material>("Materials/Route");
            lineRenderer.SetWidth(5.0f, 5.0f);
            networkLines.Add(line);
            lineRenderer.enabled = false;
        }
    }

    #endregion
}

