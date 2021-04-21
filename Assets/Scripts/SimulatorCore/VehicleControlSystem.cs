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




public class VehicleControlSystem : MonoBehaviour
{

    const float AGL_700_METERS = 213.36f;
    const float AGL_1200_METERS = 365.76f;
    public float MIN_DRONE_RANGE;
    public float DRONE_SCALE = 5.0f;


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
    

    // private SceneManagerBase sceneManager;
    public CityViewManager sceneManager;
    public List<GameObject> corridorDrones;
    public List<GameObject> lowAltitudeDrones;
    public List<GameObject> movingVehicles;

    // Background drone related params
    public List<GameObject> backgroundDrones;
    public int backgroundDroneCount = 100;
    public float lowerElevationBound = 100;
    public float upperElevationBound = 135;
    public float[][] cityBounds;

    
    public List<GameObject> parkingCollection;
    public List<GameObject> landingCollection;
    public Dictionary<GameObject, GameObject> parkingLandingMapping;


    public List<GameObject> networkLines;
    public bool networkGenerated;

    public float speedMultiplier = 1.0f;

    public GameObject TypeAPrefab;
    



    #endregion




    private string current_runtime;
    public DroneInstantiator droneInstantiator;


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
        corridorDrones = new List<GameObject>();
        lowAltitudeDrones = new List<GameObject>();
        movingVehicles = new List<GameObject>();
        
        

        speedMultiplier = 2.0f;

        watch = 0.0f;

        var eM = EnvironManager.Instance;
        var city = eM.GetCurrentCity();
        cityBounds = UnitUtils.GetCityExtents(city.CityStats);

        droneInstantiator = new DroneInstantiator(this);
        droneInstantiator.ReadVehicleSpecs();
        droneInstantiator.InstantiateBackgroundDrones(sceneManager, backgroundDroneCount, DRONE_SCALE, lowerElevationBound, upperElevationBound);
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
            if (!vehicleInstantiated)
            {
                corridorDrones = droneInstantiator.InstantiateCorridorDrones(sceneManager, DRONE_SCALE);
                // From here : this gets extremely slow...
                lowAltitudeDrones = droneInstantiator.InstantiateLowAltitudeDrones(sceneManager, DRONE_SCALE);
                vehicleInstantiated = true;
                foreach (GameObject gO in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (gO.name.Contains("Building"))
                    {
                        gO.AddComponent<BuildingNoise>();
                        gO.layer = 9;
                        gO.tag = "Building";
                    }
                }
            }
            if(!networkGenerated)
            {
                VisualizeNetwork(sceneManager.network);
                networkGenerated = true;
            }
        }
    }

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
                    Debug.Log("No available vehicle");
                    return;
                }
                destinations_list.Add(parking);
                Queue<GameObject> destinations = Route(parking, destinations_list);
                GameObject vehicle = GetAvailableVehicleinParkingStrcuture(parking);
                if (vehicle == null)
                {
                    Debug.Log("No available vehicle");
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





    private void CallVehicle(GameObject vehicle, ParkingControl parking, Queue<GameObject> destinations)
    {
        DroneBase vehicleInfo = vehicle.GetComponent<DroneBase>();
        if(destinations != null) vehicleInfo.destinationQueue = destinations;
        parking.CallVehecleInParkingStructure(vehicle);
    }

    public float GetElevation(GameObject origin, GameObject destination)
    {
        // TO-DO: Assign elevation according to the simulation rules
        // Now: All 100m to test obstacle avoidance
        if (origin.transform.position.y > 100.0f) return origin.transform.position.y + 50.0f;
        return 152.0f;
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

    public GameObject GetNearestAvailableParking(Vector3 AAOCenter)
    {
        GameObject closestParking = null;

        float minDistance = Mathf.Infinity;
        // For all parking structures
        foreach (var sPS in sceneManager.ParkingStructures.Values)
        {
            var gO = sPS.gameObject;
            // find the nearest one with parked vehicles
            if (sPS.ParkingCtrl.parkingInfo.VehicleAt.Keys.Count > 0 && sPS.ParkingStructureSpecs.Type.Contains("LowAltitude") && sPS.ParkingCtrl.queueLength < 2)
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
        }
    }

    #endregion
}

