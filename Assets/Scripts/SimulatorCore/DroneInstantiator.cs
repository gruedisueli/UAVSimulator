using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Environment;
using Assets.Scripts.UI;
using Assets.Scripts.DataStructure;
using System.Xml.Linq;
using System.IO;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.Serialization;

namespace Assets.Scripts.SimulatorCore
{
    /// <summary>
    /// Manages the actual instantiation of drone game objects in the simulation.
    /// </summary>
    public class DroneInstantiator: MonoBehaviour
    {

        private VehicleControlSystem vcs;
        private Dictionary<string, VehicleSpec> vehicleSpecs;//list of specs for different types of drones. @Eunu, correct?
        private Dictionary<string, string> vehicleTypes;//list of possible types of vehicles. @Eunu, Correct? Do these become the keys for vehicle specs? What are the key value pairs in this dictionary?
        private string asset_root = SerializationSettings.ROOT + "\\";
        
        public bool isCorridorDroneInstantiated;
        public bool isLowAltitudeDroneInstantiated;
        public bool isBackgroundDroneInstantiated;

        public bool corridorDroneInstantiationStarted;
        public bool lowAltitudeDroneInstantiationStarted;

        public List<GameObject> backgroundDrones;
        public List<GameObject> corridorDrones;
        public List<GameObject> lowAltitudeDrones;

        public event EventHandler<DroneInstantiationArgs> OnDroneInstantiated;
       

        public void Init(VehicleControlSystem vcs)
        {
            vehicleSpecs = new Dictionary<string, VehicleSpec>();
            vehicleTypes = new Dictionary<string, string>();
            backgroundDrones = new List<GameObject>();
            corridorDrones = new List<GameObject>();
            lowAltitudeDrones = new List<GameObject>();
            this.vcs = vcs;
            ReadVehicleSpecs();
        }

        /// <summary>
        /// Main method called for instantiating drones in the simulation.
        /// </summary>
        public void InstantiateDrones(SceneManagerBase sceneManager, float scale, Canvas _canvas)
        {
            StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, true));
            StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, false));
        }

        ///// <summary>
        ///// Coroutine for instantiation of corridor drones.
        ///// </summary>
        //public IEnumerator InstantiateCorridorDrones(CityViewManager sceneManager, float scale, Canvas _canvas)
        //{
            
        //    string drone_path = "Drones/";
        //    float progress = 0.0f;
        //    List<GameObject> vehicles = new List<GameObject>();
        //    int parkingCapacity = sceneManager.GetParkingCapacity();
        //    int lowAltitudeOnlyCapacity = sceneManager.GetParkingCapacity("LowAltitude");
        //    int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - lowAltitudeOnlyCapacity - 10 - corridorDrones.Count, parkingCapacity - lowAltitudeOnlyCapacity - corridorDrones.Count);

        //    if (vehiclesToInstantiate < 0) yield break;

        //    List<string> corridorTypeNames = new List<string>();

        //    var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
        //    var progressBar = pG.GetComponent<ProgressBar>();
        //    progressBar.Init("Instantiating drones");

        //    corridorDroneInstantiationStarted = true;
        

        //    foreach ( string type in vehicleTypes.Keys )
        //    {
        //        if (type.Equals("corridor")) corridorTypeNames.Add(vehicleTypes[type]);
        //    }

        //    for (int i = 0; i < vehiclesToInstantiate; i++)
        //    {
        //        foreach (var key in sceneManager.ParkingStructures.Keys)
        //        {
        //            var sPS = sceneManager.ParkingStructures[key];
        //            ParkingControl pC = sPS.ParkingCtrl;
        //            if (!sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
        //            {
        //                if (pC.parkingInfo.Parked.Count == 0) pC.parkingInfo.RemainingSpots = pC.parkingInfo.ParkingSpots.Count - pC.parkingInfo.Parked.Count;
        //                if (pC.parkingInfo.RemainingSpots > 0)
        //                {
        //                    int vehicleTypeID = UnityEngine.Random.Range(0, corridorTypeNames.Count);
        //                    var newDrone = Resources.Load<GameObject>(drone_path + corridorTypeNames[vehicleTypeID]);
        //                    var type = corridorTypeNames[vehicleTypeID];
        //                    var emptySpot = pC.parkingInfo.GetEmptySpot();
        //                    var translatedSpot = pC.parkingInfo.TranslateParkingSpot(emptySpot);

        //                    // instantiate the vehicle at emptySpot
        //                    var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        //                    clone.name = "UAV_" + i.ToString();
        //                    clone.tag = "Vehicle";
        //                    clone.layer = 10;
        //                    // Only for video
        //                    clone.transform.localScale = new Vector3(scale, scale, scale);
        //                    // ~Only for video
        //                    clone.AddComponent<VehicleNoise>();
        //                    TrailRenderer tr = clone.AddComponent<TrailRenderer>();
        //                    tr.material = Resources.Load<Material>("Materials/TrailCorridorDrones");
        //                    tr.time = Mathf.Infinity;
        //                    tr.enabled = false;


        //                    //newDrone.Destroy();

        //                    // Fill in vehivle spec
        //                    CorridorDrone v = clone.AddComponent<CorridorDrone>();
        //                    v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "idle");
        //                    v.currentCommunicationPoint = sPS.gameObject;
        //                    vehicles.Add(clone);
        //                    OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
        //                    // Update parking management info
        //                    clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
        //                    pC.parkingInfo.ParkAt(emptySpot, clone);

        //                    var sc = clone.AddComponent<SphereCollider>();
        //                    sc.radius = 1.0f;
        //                    sc.center = Vector3.zero;

        //                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //                    sphere.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        //                    sphere.transform.parent = clone.transform;
        //                    sphere.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //                    MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
        //                    mr.material = Resources.Load<Material>("Materials/NoiseSphere");
        //                    mr.enabled = false;
        //                    corridorDrones.Add(clone);
        //                    break;
        //                }
        //            }
        //        }
        //        progress = (float)i / (float)vehiclesToInstantiate;
        //        progressBar.SetCompletion(progress);
        //        yield return null;
        //    }
        //    //return vehicles;
        //    pG.Destroy();
        //    isCorridorDroneInstantiated = true;
        //}

        /// <summary>
        /// Parses vehicle specs from external file.
        /// </summary>
        public void ReadVehicleSpecs()
        {
            if (vehicleTypes.Keys.Count > 0) return;
            string path = asset_root + "Resources\\Drones\\";
            var files = Directory.GetFiles(path, "*.JSON");
            foreach (var filename in files)
            {
                string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
                VehicleSpec vs = JsonUtility.FromJson<VehicleSpec>(json);
                string name = Path.GetFileNameWithoutExtension(filename);
                vehicleTypes.Add(vs.type, name);
                vehicleSpecs.Add(name, vs);
                //if (vs.range < MIN_DRONE_RANGE) MIN_DRONE_RANGE = vs.range;
                vs.range = Mathf.Infinity;
            }
        }

        ///// <summary>
        ///// Coroutine for instantiation of low altitude drones.
        ///// </summary>
        //public IEnumerator InstantiateLowAltitudeDrones(CityViewManager sceneManager, float scale, Canvas _canvas)
        //{
        //    string drone_path = "Drones/";
        //    float progress = 0.0f;
        //    List<GameObject> vehicles = new List<GameObject>();
        //    int parkingCapacity = sceneManager.GetParkingCapacity();
        //    int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - 10, parkingCapacity);

        //    if (vehiclesToInstantiate < 0) yield break;

        //    var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
        //    var progressBar = pG.GetComponent<ProgressBar>();
        //    progressBar.Init("Instantiating drones");


        //    List<string> lowAltitudeTypeNames = new List<string>();

        //    lowAltitudeDroneInstantiationStarted = true;

        //    foreach (string type in vehicleTypes.Keys)
        //    {
        //        if (type.Equals("LowAltitude")) lowAltitudeTypeNames.Add(vehicleTypes[type]);
        //    }

        //    // Populate vehiclesToInstantiate number of drones in existing parking structures
        //    for (int i = 0; i < vehiclesToInstantiate; i++)
        //    {
        //        foreach (var key in sceneManager.ParkingStructures.Keys)
        //        {
        //            var sPS = sceneManager.ParkingStructures[key];
        //            ParkingControl pC = sPS.ParkingCtrl;
        //            if (sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
        //            {
        //                if (pC.parkingInfo.Parked.Count == 0) pC.parkingInfo.RemainingSpots = pC.parkingInfo.ParkingSpots.Count - pC.parkingInfo.Parked.Count;
        //                if (pC.parkingInfo.RemainingSpots > 0)
        //                {
        //                    int vehicleTypeID = UnityEngine.Random.Range(0, lowAltitudeTypeNames.Count);
        //                    var newDrone = Resources.Load<GameObject>(drone_path + lowAltitudeTypeNames[vehicleTypeID]);
        //                    var type = lowAltitudeTypeNames[vehicleTypeID];
        //                    var emptySpot = pC.parkingInfo.GetEmptySpot();
        //                    var translatedSpot = pC.parkingInfo.TranslateParkingSpot(emptySpot);

        //                    // instantiate the vehicle at emptySpot
        //                    var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        //                    clone.name = "UAV_" + i.ToString();
        //                    clone.tag = "Vehicle";
        //                    clone.layer = 10;
        //                    // Only for video
        //                    clone.transform.localScale = new Vector3(scale, scale, scale);
        //                    // ~Only for video
        //                    clone.AddComponent<VehicleNoise>();
        //                    TrailRenderer tr = clone.AddComponent<TrailRenderer>();
        //                    tr.material = Resources.Load<Material>("Materials/TrailLowAltitudeDrone");
        //                    tr.time = Mathf.Infinity;
        //                    tr.enabled = false;

        //                    var sc = clone.AddComponent<SphereCollider>();
        //                    sc.radius = 1.0f;
        //                    sc.center = Vector3.zero;


        //                    //newDrone.Destroy();

        //                    // Fill in vehivle spec
        //                    LowAltitudeDrone v = clone.AddComponent<LowAltitudeDrone>();
        //                    v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "idle");
        //                    v.currentCommunicationPoint = sPS.gameObject;
        //                    v.approaching_threshold = 100.0f;
        //                    vehicles.Add(clone);
        //                    OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
        //                    // Update parking management info
        //                    clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                            
        //                    pC.parkingInfo.ParkAt(emptySpot, clone);

        //                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //                    sphere.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        //                    sphere.transform.parent = clone.transform;
        //                    sphere.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //                    MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
        //                    mr.material = Resources.Load<Material>("Materials/NoiseSphere");
        //                    mr.enabled = false;

        //                    lowAltitudeDrones.Add(clone);

        //                    break;
        //                }
        //            }
        //        }
        //        progress = (float)i / (float)vehiclesToInstantiate;
        //        progressBar.SetCompletion(progress);
        //        yield return null;
        //    }
        //    pG.Destroy();
        //    isLowAltitudeDroneInstantiated = true;
        //    //return vehicles;
        //}

        /// <summary>
        /// Coroutine for instantiation of background drones. @Eunu please comment on when we use this versus "AddBackgroundDrone()"
        /// </summary>
        public IEnumerator InstantiateBackgroundDrones(SceneManagerBase sceneManager, int backgroundDroneCount, float scale, float lowerElevationBound, float upperElevationBound, Canvas _canvas)
        {
            float progress = 0.0f;
            int vehiclesToInstantiate = backgroundDroneCount;
            string drone_path = vcs.IsRegionView ? "Sprites/DroneSprite" : "Drones/";
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            // Populate vehiclesToInstantiate number of drones in existing parking structures

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("Initializing Simulation");

            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated

                int vehicleTypeID = UnityEngine.Random.Range(0, vehicleSpecs.Keys.Count);
                string path = vcs.IsRegionView ? drone_path : drone_path + vehicleSpecs.Keys.ToList()[vehicleTypeID];
                var newDrone = Resources.Load<GameObject>(path);
                var type = vehicleSpecs.Keys.ToList()[vehicleTypeID];

                float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
                Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
                // instantiate the vehicle at emptySpot
                var clone = Instantiate(newDrone, instantiationSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                clone.name = "UAV_BACKGROUND_" + backgroundDrones.Count.ToString();
                clone.tag = "Vehicle";
                clone.layer = 10;

                // Only for video
                clone.transform.localScale = new Vector3(scale, scale, scale);
                // ~Only for video
                //clone.AddComponent<VehicleNoise>();
                TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                tr.startColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
                tr.endColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
                tr.material = backgroundTrail;
                tr.time = 60.0f;
                //newDrone.Destroy();

                // Fill in vehivle spec
                BackgroundDrone v = clone.AddComponent<BackgroundDrone>();
                v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "background");
                //v.currentPoint = v.gameObject.transform.posit;
                v.isBackgroundDrone = true;
                Vector3 firstDestination = vcs.GetRandomPointXZ(y);
                v.wayPointsQueue = new Queue<Vector3>(sceneManager.FindPath(instantiationSpot, firstDestination, 5, 1 << 8 | 1 << 9 | 1 << 13));
                v.targetPosition = v.wayPointsQueue.Dequeue();
                backgroundDrones.Add(clone);

                // Update parking management info

                progress = (float)i / (float)vehiclesToInstantiate;
                progressBar.SetCompletion(progress);
                yield return null;
            }
            pG.Destroy();
            isBackgroundDroneInstantiated = true;
        }

        /// <summary>
        /// Main coroutine for instantiating drones of low altitude or corridor type.
        /// </summary>
        private IEnumerator InstantiateCorridorOrLowAltDrones(SceneManagerBase sceneManager, float scale, Canvas _canvas, bool isCorridor)
        {
            string drone_path = vcs.IsRegionView ? "Sprites/DroneSprite" : "Drones/";
            string droneType = isCorridor ? "corridor" : "LowAltitude";
            float progress = 0.0f;
            int parkingCapacity = sceneManager.GetParkingCapacity();
            int typeSpecificCapacity = sceneManager.GetParkingCapacity(droneType);
            int currentDroneCt = isCorridor ? corridorDrones.Count : lowAltitudeDrones.Count;
            int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - typeSpecificCapacity - 10 - currentDroneCt, parkingCapacity - typeSpecificCapacity - currentDroneCt);

            if (vehiclesToInstantiate < 0) yield break;

            List<string> typeNames = new List<string>();

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("Instantiating drones");

            if (isCorridor) corridorDroneInstantiationStarted = true;
            else lowAltitudeDroneInstantiationStarted = true;



            foreach (string type in vehicleTypes.Keys)
            {
                if (type.Equals(droneType)) typeNames.Add(vehicleTypes[type]);
            }

            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                foreach (var key in sceneManager.ParkingStructures.Keys)
                {
                    var sPS = sceneManager.ParkingStructures[key];
                    ParkingControl pC = sPS.ParkingCtrl;
                    bool isLE = sPS.ParkingStructureSpecs.Type.Contains("LowAltitude");
                    if (isCorridor ? !isLE : isLE)
                    {
                        if (pC.RemainingSpots > 0)
                        {
                            int vehicleTypeID = UnityEngine.Random.Range(0, typeNames.Count);
                            string path = vcs.IsRegionView ? drone_path : drone_path + typeNames[vehicleTypeID];
                            var newDrone = Resources.Load<GameObject>(path);
                            var type = typeNames[vehicleTypeID];
                            var emptySpot = pC.GetEmptySpot();
                            var translatedSpot = pC.TranslateParkingSpot(emptySpot);

                            // instantiate the vehicle at emptySpot
                            var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                            clone.name = "UAV_" + (isCorridor ? corridorDrones.Count.ToString() : lowAltitudeDrones.Count.ToString());
                            clone.tag = "Vehicle";
                            clone.layer = 10;
                            // Only for video
                            clone.transform.localScale = new Vector3(scale, scale, scale);
                            // ~Only for video
                            var vN = clone.AddComponent<VehicleNoise>();
                            vN.Init(vcs);
                            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                            tr.material = Resources.Load<Material>("Materials/TrailCorridorDrones");
                            tr.time = Mathf.Infinity;
                            tr.enabled = false;

                            // Fill in vehicle spec
                            DroneBase v = isCorridor ? (DroneBase)clone.AddComponent<CorridorDrone>() : (DroneBase)clone.AddComponent<LowAltitudeDrone>();
                            v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "idle");
                            v.currentCommunicationPoint = sPS.gameObject;
                            OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
                            // Update parking management info
                            if (!vcs.IsRegionView) clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                            else
                            {
                                clone.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                            }
                            pC.ParkAt(emptySpot, clone);

                            var sc = clone.AddComponent<SphereCollider>();
                            sc.radius = 1.0f;
                            sc.center = Vector3.zero;

                            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            sphere.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                            sphere.transform.parent = clone.transform;
                            sphere.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                            MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
                            mr.material = Resources.Load<Material>("Materials/NoiseSphere");
                            mr.enabled = false;
                            if (isCorridor) corridorDrones.Add(clone);
                            else lowAltitudeDrones.Add(clone);
                            break;
                        }
                    }
                }
                progress = (float)i / (float)vehiclesToInstantiate;
                progressBar.SetCompletion(progress);
                yield return null;
            }
            //return vehicles;
            pG.Destroy();
            if (isCorridor) isCorridorDroneInstantiated = true;
            else isLowAltitudeDroneInstantiated = true;
        }

        /// <summary>
        /// @Eunu, comment. See comment for coroutine above.
        /// </summary>
        public void AddBackgroundDrone(SceneManagerBase sceneManager, float scale, float lowerElevationBound, float upperElevationBound)
        {
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            // Populate vehiclesToInstantiate number of drones in existing parking structures


            // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated

            int vehicleTypeID = UnityEngine.Random.Range(0, vehicleSpecs.Keys.Count);
            string drone_path = vcs.IsRegionView ? "Sprites/DroneSprite" : "Drones/" + vehicleSpecs.Keys.ToList()[vehicleTypeID];

            var newDrone = Resources.Load<GameObject>(drone_path);
            var type = vehicleSpecs.Keys.ToList()[vehicleTypeID];

            float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
            Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
            // instantiate the vehicle at emptySpot
            var clone = Instantiate(newDrone, instantiationSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            clone.name = "UAV_BACKGROUND_" + backgroundDrones.Count.ToString();
            clone.tag = "Vehicle";
            clone.layer = 10;

            // Only for video
            clone.transform.localScale = new Vector3(scale, scale, scale);
            // ~Only for video
            //clone.AddComponent<VehicleNoise>();
            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
            tr.startColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.endColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.material = backgroundTrail;
            tr.time = 60.0f;
            //newDrone.Destroy();

            // Fill in vehivle spec
            BackgroundDrone v = clone.AddComponent<BackgroundDrone>();
            v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "background");
            //v.currentPoint = v.gameObject.transform.posit;
            v.isBackgroundDrone = true;
            Vector3 firstDestination = vcs.GetRandomPointXZ(y);
            v.wayPointsQueue = new Queue<Vector3>(sceneManager.FindPath(instantiationSpot, firstDestination, 5, 1 << 8 | 1 << 9 | 1 << 13));
            v.targetPosition = v.wayPointsQueue.Dequeue();

            backgroundDrones.Add(clone);
        }
    }

}
