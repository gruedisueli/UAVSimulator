using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts;
using Assets.Scripts.DataStructure;

public class EnvironmentManagement : MonoBehaviour
{
    public Material restrictionZoneMaterial;
    const float RANGE_LIMIT = 4000.0f;
    string root;
    string current_runtime;

    Dictionary<string, DronePort> dronePortSpecs;
    Dictionary<string, List<Vector3>> parkingSpots;

    public Dictionary<GameObject, DronePort> dronePorts;
    public Dictionary<GameObject, ParkingStructure> parkingStructures;

    public List<GameObject> destinationCollections;
    public Dictionary<GameObject, List<GameObject>> routes;

    List<GameObject> restrictionZones;
    // Start is called before the first frame update
    void Start()
    {
        restrictionZones = new List<GameObject>();
        dronePortSpecs = new Dictionary<string, DronePort>();
        parkingSpots = new Dictionary<string, List<Vector3>>();
        // Temporary
        root = "E:\\Development\\GitHub_Local\\UAVSimulator\\";
        current_runtime = "42o3601_71o0589";
        // ~Temporary


        // Temporary

        dronePorts = new Dictionary<GameObject, DronePort>();
        parkingStructures = new Dictionary<GameObject, ParkingStructure>();
        routes = new Dictionary<GameObject, List<GameObject>>();

        readDronePortSpecs();
        // readVehicleSpecs();
        readParkingStructure();
        Environment env = readEnvironment();
        instantiateObjects(env);

        createRoutes();

    }

    // Update is called once per frame
    void Update()
    {
        // Check if the user paused simulation and designated more drone ports/parking structures/restriction zones
        // Update the information correspondingly
    }

    void createRoutes()
    {
        
        destinationCollections = new List<GameObject>();
        foreach (GameObject dp in dronePorts.Keys)
        {
            destinationCollections.Add(dp);
        }


        // create straight paths first (elevation == 0 means straight paths)
        // paths whose elevation is closest to the assigned elevation will be examined first
        // if there is an obstacle in the middle, construct a walkaround path and register with the new elevation
        
        for (int i = 0; i < destinationCollections.Count; i++)
        {
            for (int j = 0; j < destinationCollections.Count; j++)
            {
                if (i != j)
                {
                    GameObject origin = destinationCollections[i];
                    GameObject destination = destinationCollections[j];
                    if (Vector3.Distance(origin.transform.position, destination.transform.position) < RANGE_LIMIT)
                    {
                        if (!routes.ContainsKey(origin)) routes.Add(origin, new List<GameObject>());
                        
                        List<GameObject> this_origin_adjacent_nodes = routes[origin];
                        this_origin_adjacent_nodes.Add(destination);
                    }
                    
                }
            }
        }

    }
  
    void readParkingStructure()
    {
        string path = root + "\\assets\\Resources\\ParkingStructures";
        var files = Directory.GetFiles(path, "*.DAT");

        foreach (string filename in files)
        {
            // Read lines and parse DAT files
            StreamReader this_file = new StreamReader(filename);
            string type = filename.Substring(filename.LastIndexOf('\\') + 1, filename.Length - filename.LastIndexOf('\\') - 5);
            List<Vector3> spots = new List<Vector3>();
            string line;
            while((line = this_file.ReadLine()) != null)
            {
                line = line.Replace("(", "").Replace(")", "");
                var splitted = line.Split(',');
                Vector3 point = new Vector3(float.Parse(splitted[0]), float.Parse(splitted[1]), float.Parse(splitted[2]));
                spots.Add(point);
            }
            parkingSpots.Add(type, spots);
        }

    }

    void readDronePortSpecs()
    {
        string path = root + "\\assets\\Resources\\DronePorts";
        var files = Directory.GetFiles(path, "*.JSON");
        foreach (var filename in files)
        {
            string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
            DronePort dp = JsonUtility.FromJson<DronePort>(json);
            dronePortSpecs.Add(dp.type, dp);
        }
    }

    Environment readEnvironment()
    {
        string path = root + "\\runtime\\" + current_runtime;
        string filename = path + "\\" + "environment.json";
        string json = File.ReadAllText(filename, System.Text.Encoding.UTF8).Replace("\r","").Replace("\n","").Replace(" ", "");
        Environment env = new Environment();
        JsonUtility.FromJsonOverwrite(json, env);
        return env;
    
    }

    void instantiateObjects(Environment env)
    {
        // TO-DO: Also register the restriction zones for drone ports and parking structures
        string path = "DronePorts/";
    
        // Instantiate different types of drone ports in the correct locations
        foreach(DronePort dp in env.dronePorts)
        {
            if(!dp.type.Contains("generic"))
            {
                // Instantiate drone port game object from obj stored in /assets/resources/droneports
                var newObject = Resources.Load<GameObject>(path + dp.type);
                var clone = Instantiate(newObject, dp.position, Quaternion.Euler(dp.rotation.x, dp.rotation.y, dp.rotation.z));
                clone.name = "DronePort_" + dp.type;
                clone.tag = "DronePort";
                clone.layer = 12;
                // Fill in type specific informations

                dp.maximumVehicleSize = dronePortSpecs[dp.type].maximumVehicleSize;
                dp.isMountable = dronePortSpecs[dp.type].isMountable;
                dp.isOnTheGround = dronePortSpecs[dp.type].isOnTheGround;
                dp.isScalable = dronePortSpecs[dp.type].isScalable;
                dronePorts.Add(clone, dp);
                DronePortControl newDronePort = clone.AddComponent<DronePortControl>();
                newDronePort.dronePortInfo = dp;
                Object.Destroy(newObject);

            }
            else
            {
                if(dp.type.Contains("rectangular"))
                {
                    // Instantiate drone port game object by creating primitive cube
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.tag = "DronePort";
                    cube.name = "DronePort_Rectangular";
                    cube.layer = 12;
                    cube.transform.position = dp.position;
                    cube.transform.rotation = Quaternion.Euler(dp.rotation.x, dp.rotation.y, dp.rotation.z);
                    cube.transform.localScale = dp.scale;
                    // Fill in type specific information
                    dp.isMountable = true;
                    dp.isOnTheGround = true;
                    dp.isScalable = true;
                    

                    // TO-DO: Update this to be the actual number
                    dp.maximumVehicleSize = 0.5f * dp.scale.x;

                    DronePortControl newDronePort = cube.AddComponent<DronePortControl>();
                    newDronePort.dronePortInfo = dp;

                    dronePorts.Add(cube, dp);
                }
            }
        }

        path = "ParkingStructures/";
        foreach (ParkingStructure ps in env.parkingStructures)
        {
            if (!ps.type.Contains("generic"))
            {
                // Instantiate parking structure from obj files stored in /assets/resources
                var newObject = Resources.Load<GameObject>(path + ps.type);
                var clone = Instantiate(newObject, ps.position, Quaternion.Euler(ps.rotation.x, ps.rotation.y, ps.rotation.z));
                clone.tag = "ParkingStructure";
                clone.name = "Parking_" + ps.type;
                clone.layer = 11;

                // Fill in type specific information
                ps.parkingSpots = new List<Vector3>(parkingSpots[ps.type]);
                ps.remainingSpots = ps.parkingSpots.Count;
                parkingStructures.Add(clone, ps);

                Parking newStructure = clone.AddComponent<Parking>();
                ps.parked = new Dictionary<Vector3, GameObject>();
                ps.vehicleAt = new Dictionary<GameObject, Vector3>();
                ps.reserved = new Dictionary<GameObject, Vector3>();
                newStructure.parkingInfo = ps;

                

                Object.Destroy(newObject);
            }
            else
            {
                if (ps.type.Contains("rectangular"))
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.tag = "ParkingStructure";
                    cube.name = "Parking_Rectangular";
                    cube.layer = 11;
                    cube.transform.position = ps.position;
                    cube.transform.rotation = Quaternion.Euler(ps.rotation.x, ps.rotation.y, ps.rotation.z);
                    cube.transform.localScale = ps.scale;

                    //TO-DO: Use real numbers for margins - now 20m

                    int parkingMargin = 20;
                    List<Vector3> spots = new List<Vector3>();
                    for ( int i = (int)(-(ps.scale.x / 2)) + parkingMargin; i <= (int)(ps.scale.x / 2) - parkingMargin; i += parkingMargin )
                    {
                        for (int j = (int)(-(ps.scale.z / 2 )) + parkingMargin; j <= (int)(ps.scale.z / 2) - parkingMargin; j += parkingMargin)
                        {
                            Vector3 v = new Vector3((float)i, 0.0f, (float)j);
                            spots.Add(v);
                        }
                    }
                    ps.parkingSpots = spots;
                    ps.remainingSpots = ps.parkingSpots.Count;

                    Parking newStructure = cube.AddComponent<Parking>();
                    ps.parked = new Dictionary<Vector3, GameObject>();
                    ps.vehicleAt = new Dictionary<GameObject, Vector3>();
                    ps.reserved = new Dictionary<GameObject, Vector3>();
                    newStructure.parkingInfo = ps;

                 
                    parkingStructures.Add(cube, ps);
                }
            }
        }

        path = "RestrictionZones/";
        foreach(RestrictionZone rz in env.restrictionZones)
        {
            if (rz.type.Contains("generic"))
            {
                GameObject newZone = null;
                if(rz.type.Contains("rectangular"))
                {
                    newZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newZone.transform.localScale = new Vector3(rz.scale.x, rz.height, rz.scale.z);
                }
                else
                {
                    newZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    newZone.transform.localScale = new Vector3(rz.scale.x, rz.height / 2, rz.scale.z);
                }
                newZone.transform.position = new Vector3(rz.position.x, rz.height / 2, rz.position.z);
                newZone.transform.rotation = Quaternion.Euler(rz.rotation.x, rz.rotation.y, rz.rotation.z);
                newZone.name = "RestrictionZone_" + rz.type;
                newZone.tag = "RestrictionZone";
                newZone.layer = 8;
                newZone.AddComponent<MeshRenderer>();
                newZone.GetComponent<MeshRenderer>().material = restrictionZoneMaterial;
                restrictionZones.Add(newZone);
            }
            else if (rz.type.Contains("Class"))
            {
                rz.bottoms.Add(rz.height);
                for(int i = 0; i < rz.bottoms.Count - 1; i++)
                {
                    float this_cylinder_center_y = (rz.bottoms[i] + rz.bottoms[i + 1]) / 2.0f;
                    float this_cylinder_height_half = (rz.bottoms[i + 1] - rz.bottoms[i]) / 2.0f;
                    float this_cylinder_radius = rz.radius[i];
                    GameObject newZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    newZone.transform.position = new Vector3(rz.position.x, this_cylinder_center_y, rz.position.z);
                    newZone.transform.localScale = new Vector3(this_cylinder_radius, this_cylinder_height_half, this_cylinder_radius);
                    newZone.name = "RestrictionZone_" + rz.type + "_" + i.ToString();
                    newZone.tag = "RestrictionZone";
                    newZone.layer = 8;
                    //newwZone.AddComponent<MeshRenderer>();
                    newZone.GetComponent<MeshRenderer>().material = restrictionZoneMaterial;
                    restrictionZones.Add(newZone);
                }
            }
        }
    }

    public int GetParkingCapacity()
    {
        int parking_capacity = 0;
        foreach(GameObject ps in parkingStructures.Keys)
        {
            parking_capacity += parkingStructures[ps].parkingSpots.Count;
        }
        return parking_capacity;
    }
    
}
