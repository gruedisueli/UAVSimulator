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



/// <summary>
/// Sort of a main control system for the entire fleet of drones in the simulation.
/// </summary>
public class VehicleControlSystem : MonoBehaviour
{

    const float AGL_700_METERS = 213.36f;//AGL 700 in meters, not feet. "700" is a feet measurement.
    const float AGL_1200_METERS = 365.76f;//AGL 1200 in meters, not feet. "1200" is a feet measurement.
    public float MIN_DRONE_RANGE;//@EUNU comment
    public float DRONE_SCALE = 5.0f;//@EUNU comment
    public Canvas _canvas;


    #region UI Variables

    public SimulationParam simulationParam;
    private Dictionary<string, float> statistics;//@EUNU remove/not used?


    #endregion

    #region Vehicle Info
    private Dictionary<GameObject, string> activeVehicles;//@EUNU comment. Does this include parked drones?
    #endregion



    #region Private Variables
    private Color translucentRed;
    private float watch;
    private int droneCount;
    
    private bool buildingNoiseAttachmentStarted;
    private bool isBuildingNoiseComponentAttached;

    // Visualization related params
    public bool playing;
    public bool noiseVisualization;
    public bool privacyVisualization;
    public bool routeVisualization;
    public bool landingCorridorVisualization;
    public bool demographicVisualization;
    public bool trailVisualization;

    private bool _simplifiedMeshToggle;
    public delegate void OnSimplifiedMeshToggleDelegate(bool toggle, Mesh m);
    public event OnSimplifiedMeshToggleDelegate OnSimplifiedMeshToggle;
    public Mesh simplifiedMesh;
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


    // INTEGRATION TO-DO: Replace root with the path that it receives;
    private string root; 
    

    // private SceneManagerBase sceneManager;
    public CityViewManager sceneManager;
    public List<GameObject> corridorDrones;
    public List<GameObject> lowAltitudeDrones;
    

    // Background drone related params
    public int backgroundDroneCount = 100;
    public float lowerElevationBound = 100;
    public float upperElevationBound = 135;
    public float[][] cityBounds;

    
    public List<GameObject> parkingCollection;
    public List<GameObject> landingCollection;
    public Dictionary<GameObject, GameObject> parkingLandingMapping;


    public List<GameObject> networkLines;
    public bool networkGenerated;
    private Dictionary<Corridor, GameObject> routeLineObject;

    public float speedMultiplier = 1.0f;//@EUNU comment

    public GameObject TypeAPrefab;

    



    #endregion

    private string current_runtime;//@EUNU comment. remove?
    private float progress = 0.0f;
    private List<GameObject> hiddenDrones;//@EUNU comment
    public DroneInstantiator droneInstantiator;


    // Start is called before the first frame update
    void Start()
    {
        root = SerializationSettings.ROOT + "\\runtime\\";
        droneCount = 200;
        playing = false;
        networkGenerated = false;
        translucentRed = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.50f);

        MIN_DRONE_RANGE = 99999.0f;//@EUNU comment
        string current_runtime = "42o3601_71o0589"; // INTEGRATION TO-DO: Get current runtime name from UI side to find out which folder to refer to
                                                    // Current Placeholder = Lat_Long of Boston

        
        simulationParam = ReadSimulationParams(current_runtime);
        // TO-DO: Add clause that checks if we are doing simulation in RegionView
        sceneManager = GameObject.Find("FOA").GetComponent<CityViewManager>();
        corridorDrones = new List<GameObject>();
        lowAltitudeDrones = new List<GameObject>();
        routeLineObject = new Dictionary<Corridor, GameObject>();
        hiddenDrones = new List<GameObject>();


        speedMultiplier = 2.0f;//@EUNU comment

        watch = 0.0f;

        var eM = EnvironManager.Instance;
        var city = eM.GetCurrentCity();
        cityBounds = UnitUtils.GetCityExtents(city.CityStats);


        

        droneInstantiator = new DroneInstantiator(this);
        droneInstantiator.ReadVehicleSpecs();
        

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        simplifiedMesh = cube.GetComponent<MeshFilter>().mesh;
        cube.Destroy();

    }

    // Update is called once per frames
    void Update()
    {
        var debug = sceneManager.RestrictionZones.Values.ElementAt<SceneRestrictionZone>(0).GetComponent<SelectableGameObject>();//@EUNU comment/remove?

        #region Basic Initialization Stuff. //@EUNU comment why we don't do this in setup? Seems like something that only happens once?

        if (!buildingNoiseAttachmentStarted) StartCoroutine(AttachBuildingNoiseComponent());
        if (isBuildingNoiseComponentAttached && !droneInstantiator.corridorDroneInstantiationStarted ) StartCoroutine(droneInstantiator.InstantiateCorridorDrones(sceneManager, DRONE_SCALE, _canvas));
        else if (isBuildingNoiseComponentAttached && droneInstantiator.isCorridorDroneInstantiated && !droneInstantiator.lowAltitudeDroneInstantiationStarted ) StartCoroutine(droneInstantiator.InstantiateLowAltitudeDrones(sceneManager, DRONE_SCALE, _canvas));

        #endregion

        #region Typical play actions

        if (playing && isBuildingNoiseComponentAttached && droneInstantiator.isCorridorDroneInstantiated && droneInstantiator.isLowAltitudeDroneInstantiated )
        {
            watch += Time.deltaTime;
            //periodic check-in
            if (watch > simulationParam.callGenerationInterval)
            {
                var sa = gameObject.GetComponent<SimulationAnalyzer>();
                //make sure we don't have excess drones in simulation.
                if (sa.flyingDrones.Count + droneInstantiator.backgroundDrones.Count >= droneCount && droneInstantiator.backgroundDrones.Count > 0)
                {
                    var droneToRemove = droneInstantiator.backgroundDrones[0];
                    droneInstantiator.backgroundDrones.RemoveAt(0);
                    droneToRemove.Destroy();
                }
                watch = 0.0f;
                GenerateRandomCalls();//@EUNU comment
            }
        }

        #endregion
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
            if ( !droneInstantiator.isBackgroundDroneInstantiated ) StartCoroutine (droneInstantiator.InstantiateBackgroundDrones(sceneManager, backgroundDroneCount, DRONE_SCALE, lowerElevationBound, upperElevationBound, _canvas));
            if (!networkGenerated)
            {
                VisualizeNetwork(sceneManager.network);
                networkGenerated = true;
            }
        }
    }

    /// <summary>
    /// Coroutine that iterates through all the objects in scene and attaches noise components to those things that should get it.
    /// </summary>
    private IEnumerator AttachBuildingNoiseComponent()
    {
        int totalChildren = 0;
        int done = 0;
        Transform citySimulatorMapTransform = GameObject.Find("CitySimulatorMap").transform;

        var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
        var progressBar = pG.GetComponent<ProgressBar>();
        progressBar.Init("Adding Noise Calculation Components");
        buildingNoiseAttachmentStarted = true;
        foreach (Transform child in citySimulatorMapTransform)
            totalChildren += child.childCount;

        foreach (Transform child in citySimulatorMapTransform)
        {
            foreach (Transform grandchild in child)
            {
                //Debug.Log("The name of current object:" + grandchild.gameObject.name);
                //if (grandchild.gameObject.name.Contains("Buildings"))
                //{
                var bN = grandchild.gameObject.AddComponent<BuildingNoise>();
                grandchild.gameObject.layer = 9;
                grandchild.gameObject.tag = "Building";
                    
                //}
                done++;
                progress = (float)done / (float)totalChildren;
                progressBar.SetCompletion(progress);
                yield return null;
            }
        }

        isBuildingNoiseComponentAttached = true;
        pG.Destroy();
    }

    /// <summary>
    /// Generates random calls for drone actions (only low-altitude and corridor drones, not background). //@EUNU is this corret?
    /// </summary>
    public void GenerateRandomCalls()
    {
        int call_type = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 3.0f));
        string call_type_string = call_type <= 0 ? "corridor" : "low-altitude";

        // strategicDeconfliction == "none"
        if (simulationParam.strategicDeconfliction.Equals("none"))
        {
            if (call_type_string.Equals("corridor"))
            {
                List<GameObject> destinations_list = GetNewRandomDestinations();
                GameObject parking = GetNearestAvailableParking(destinations_list[0]);
                if (parking == null)
                {
                    //Debug.Log("No available vehicle");
                    return;
                }
                destinations_list.Add(parking);
                Queue<GameObject> destinations = Route(parking, destinations_list);
                GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);
                if (vehicle == null)
                {
                    //Debug.Log("No available vehicle");
                    return;
                }
                //if (parking.GetComponent<ParkingControl>().queue.Count < 3) 
                CallVehicle(vehicle, parking.GetComponent<ParkingControl>(), destinations);
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

                    Vector3 polygonCenter = GetRandomPointXZ(0.0f);
                    p.Move(polygonCenter);

                    // TO-DO: Augment vehicle type (make it find the right type) and make it possible to have more than one drone in operation per AAO
                    GameObject parking = GetNearestAvailableParking(polygonCenter);
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
                    // Generate 
                    Mesh m = p.CreateExtrusion(simulationParam.lowAltitudeBoundary);
                    var AAO = new GameObject("AAO_" + vehicle.name);
                    /*
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
                    */
                    AAO.layer = 8;
                    AAOControl aaoCon = AAO.AddComponent<AAOControl>();
                    aaoCon.AddVehicle(vehicle);
                    

                    List<Vector3> generatedPoints = p.GeneratePointsinExtrusion(aaoCon.GetVehicleCount(), simulationParam.lowAltitudeBoundary);
                    generatedPoints.Add(parking.GetComponent<ParkingControl>().parkingInfo.StandbyPosition + parking.transform.position);
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
        if(destinations != null) vehicleInfo.destinationQueue = destinations;
        parking.CallVehecleInParkingStructure(vehicle);
    }

    /// <summary>
    /// //@EUNU comment.
    /// </summary>
    public float GetElevation(GameObject origin, GameObject destination)
    {
        // TO-DO: Assign elevation according to the simulation rules
        // Now: All 100m to test obstacle avoidance
        if (origin.transform.position.y > 100.0f) return origin.transform.position.y + 50.0f;
        return 152.0f;
    }

    /// <summary>
    /// Gives us a random point on the map. //@EUNU correct?
    /// </summary>
    public Vector3 GetRandomPointXZ(float y)
    {
        
        float x = UnityEngine.Random.Range(cityBounds[0][0], cityBounds[0][1]);
        float z = UnityEngine.Random.Range(cityBounds[1][0], cityBounds[1][1]);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// //@EUNU comment. is this called?
    /// </summary>
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

    /// <summary>
    /// Parses simulation params from options file.
    /// </summary>
    private SimulationParam ReadSimulationParams(string runtime_name)
    {
        string path = root + runtime_name + "\\" + "simulation.json";
        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        SimulationParam sp = JsonUtility.FromJson<SimulationParam>(json);

        return sp;
    }

    /// <summary>
    /// Finds an available vehicle in the parking structure.
    /// </summary>
    public GameObject GetAvailableVehicleinParkingStrcuture(GameObject parkingStructure)
    {
        ParkingControl pC = parkingStructure.GetComponent<ParkingControl>();
        for ( int i = 0; i < pC.parkingInfo.VehicleAt.Keys.Count; i++)
        {
            GameObject vehicle_i = pC.parkingInfo.VehicleAt.Keys.ElementAt<GameObject>(i);
            DroneBase db = vehicle_i.GetComponent<DroneBase>();
            if (db != null && db.state == "idle" && db.destinationQueue.Count == 0 && !pC.QueueContains(vehicle_i))
            {
                return vehicle_i;
            }
        }
        return null;
    }

    /// <summary>
    /// Generates a route from origin through specified destinations. May go through additional intermediate points as needed.//@EUNU correct?
    /// </summary>
    public Queue<GameObject> Route (GameObject origin, List<GameObject> destinations)
    {
        Queue<GameObject> routed_destinations = new Queue<GameObject>();
        destinations.Insert(0, origin);
        for ( int i = 0; i < destinations.Count - 1; i++ )
        {
            List<GameObject> shortestRoute = Dijkstra(destinations[i], destinations[i + 1], MIN_DRONE_RANGE);
            shortestRoute.RemoveAt(0);
            foreach (GameObject gO in shortestRoute)
            {
                routed_destinations.Enqueue(gO);
            }
        }
        return routed_destinations;
    }



    /// <summary>
    /// Finds shortest path in corridor network from one point to another. @Eunu correct?
    /// Assumes a vehicle gets fully charged at each stop @Eunu are we calculating charge?
    /// </summary>
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
            foreach (Corridor outEdge in sceneManager.network.outEdges[currentNode])
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

    /// <summary>
    /// Gets some random destinations for a CORRIDOR drone. @Eunu correct?
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
    /// Finds nearest parking structure with an available drone to first destination in call. @Eunu correct?
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
            if (sPS.ParkingCtrl.parkingInfo.VehicleAt.Keys.Count > 0 && sPS.ParkingCtrl.queueLength < 3 && !sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
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
    /// Gets nearest parking structure to center of low-altitude drone path. @Eunu correct?
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
            if (sPS.ParkingCtrl.parkingInfo.VehicleAt.Keys.Count > 0 && sPS.ParkingStructureSpecs.Type.Contains("LowAltitude") && sPS.ParkingCtrl.queueLength < 3)
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
    /// @Eunu remove?
    /// </summary>
    public bool Register(DroneBase v)
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

    /// <summary>
    /// @Eunu comment
    /// </summary>
    public bool UpdateVehicleStatus(DroneBase v)
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

    /// <summary>
    /// Allows user to specify number of drones in simulation at runtime.
    /// </summary>
    public void UpdateVehicleCount(int count)
    {
        droneCount = count;
        SimulationAnalyzer sa = gameObject.GetComponent<SimulationAnalyzer>();
        if ( count < droneInstantiator.backgroundDrones.Count + sa.flyingDrones.Count)//if too many drones are in simulation, we need to remove some.
        {
            int dronesToRemove = droneInstantiator.backgroundDrones.Count + sa.flyingDrones.Count - count;
            if (droneInstantiator.backgroundDrones.Count > 0)
            {
                List<GameObject> backgroundDronesCopy = new List<GameObject>(droneInstantiator.backgroundDrones);
                for (int i = 0; i < droneInstantiator.backgroundDrones.Count + sa.flyingDrones.Count - count; i++)
                {
                    var droneToDestroy = droneInstantiator.backgroundDrones[i];
                    backgroundDronesCopy.Remove(droneToDestroy); 
                    droneToDestroy.Destroy();
                    dronesToRemove--;
                    if (backgroundDronesCopy.Count == 0) break;
                }
                droneInstantiator.backgroundDrones = backgroundDronesCopy;
                if ( dronesToRemove > 0 )
                {
                    List<GameObject> flyingDronesCopy = new List<GameObject>(sa.flyingDrones);
                    for ( int i = 0; i < dronesToRemove; i++ )
                    {
                        var droneToHide = sa.flyingDrones[i];
                        hiddenDrones.Add(droneToHide);
                        droneToHide.GetComponent<DroneBase>().HideMesh();
                        flyingDronesCopy.Remove(droneToHide);
                        if (flyingDronesCopy.Count == 0) break;
                    }
                    sa.flyingDrones = flyingDronesCopy;
                }
            }
        }
        else if ( count == droneInstantiator.backgroundDrones.Count + sa.flyingDrones.Count )//no reason to change anything.
        {
            return;
        }
        else//we should add some
        {
            int dronesToAdd = count - (droneInstantiator.backgroundDrones.Count + sa.flyingDrones.Count);
            if ( hiddenDrones.Count > 0 )
            {
                List<GameObject> hiddenDronesCopy = new List<GameObject>(hiddenDrones);
                for ( int i = 0; i < hiddenDrones.Count; i++ )
                {
                    var dronesToShow = hiddenDrones[i];
                    var droneState = dronesToShow.GetComponent<DroneBase>();
                    if (droneState.state != "idle")
                    {
                        sa.flyingDrones.Add(dronesToShow);
                        dronesToAdd--;
                        if (dronesToAdd == 0) break;
                        droneState.ShowMesh();
                        hiddenDronesCopy.Remove(dronesToShow);
                    }
                }
            }
            for ( int i = 0; i < dronesToAdd; i++)
            {
                droneInstantiator.AddBackgroundDrone(sceneManager, DRONE_SCALE, lowerElevationBound, upperElevationBound);
            }
        }

    }

    /// <summary>
    /// Allows user to toggle noise visualization at runtime.
    /// </summary>
    public void ToggleNoiseVisualization ( bool toggle )
    {
        noiseVisualization = toggle;
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
    /// Allows user to toggle route visualization at runtime.
    /// </summary>
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

        SimulationAnalyzer sa = gameObject.GetComponent<SimulationAnalyzer>();
        if (congestionLevel > 0 && !sa.congestedCorridors.Contains(c)) sa.congestedCorridors.Add(c);
        else if (congestionLevel == 0 && sa.congestedCorridors.Contains(c)) sa.congestedCorridors.Remove(c);

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
    /// @Eunu comment.
    /// </summary>
    public void VisualizeNetwork ( Assets.Scripts.DataStructure.Network network )
    {
        networkLines = new List<GameObject>();
        foreach(Corridor c in network.corridors)
        {
            GameObject line = new GameObject();
            line.name = "Corridor_" + c.origin.name + "_" + c.destination.name;
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.positionCount = c.wayPoints.Count;
            List<Vector3> wayPointsList = new List<Vector3>(c.wayPoints.ToArray());
            //wayPointsList.Insert(0, c.origin.transform.position);
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
            routeLineObject.Add(c, line);
            c.OnCongestionLevelChange += CongestionLevelChangeHandler;
        }
    }

    #endregion
}

