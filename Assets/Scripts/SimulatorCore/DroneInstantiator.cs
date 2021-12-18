using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Environment;
using Assets.Scripts.UI;
using Assets.Scripts.DataStructure;
using System.Xml.Linq;
using System.IO;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI.Tools;

namespace Assets.Scripts.SimulatorCore
{
    /// <summary>
    /// Manages the actual instantiation of drone game objects in the simulation.
    /// </summary>
    public class DroneInstantiator: MonoBehaviour
    {

        private VehicleControlSystem vcs;
        private Dictionary<string, VehicleSpec> _vehicleSpecs = new Dictionary<string, VehicleSpec>();
        private Dictionary<string, string> _vehicleTypes = new Dictionary<string, string>();
        private string asset_root = SerializationSettings.ROOT + "\\";
        
        public bool isCorridorDroneInstantiated;
        public bool isLowAltitudeDroneInstantiated;
        public bool isBackgroundDroneInstantiated;

        public bool corridorDroneInstantiationStarted;
        public bool lowAltitudeDroneInstantiationStarted;

        public Dictionary<GameObject, DroneType> _droneTypeLookup = new Dictionary<GameObject, DroneType>();
        public List<GameObject> _backgroundDrones = new List<GameObject>();
        public List<GameObject> _corridorDrones = new List<GameObject>();
        public List<GameObject> _lowAltitudeDrones = new List<GameObject>();

        public event EventHandler<DroneInstantiationArgs> OnDroneInstantiated;
       

        public void Init(VehicleControlSystem vcs)
        {
            this.vcs = vcs;
            ReadVehicleSpecs();
        }

        /// <summary>
        /// Clears out the simulation of drones.
        /// </summary>
        public void ResetDrones()
        {
            _droneTypeLookup.Clear();
            foreach(var g in _backgroundDrones)
            {
                g.Destroy();
            }
            foreach(var g in _corridorDrones)
            {
                g.Destroy();
            }
            foreach(var g in _lowAltitudeDrones)
            {
                g.Destroy();
            }
            isBackgroundDroneInstantiated = false;
        }

        /// <summary>
        /// Main method called for instantiating drones in the simulation.
        /// </summary>
        public void InstantiateDrones(SceneManagerBase sceneManager, float scale, Canvas _canvas)
        {
            StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, true));
            StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, false));
        }

        /// <summary>
        /// Parses vehicle specs from external file.
        /// </summary>
        public void ReadVehicleSpecs()
        {
            if (_vehicleTypes.Keys.Count > 0) return;
            string path = asset_root + "Resources\\Drones\\";
            var files = Directory.GetFiles(path, "*.JSON");
            foreach (var filename in files)
            {
                string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
                VehicleSpec vs = JsonUtility.FromJson<VehicleSpec>(json);
                string name = Path.GetFileNameWithoutExtension(filename);
                _vehicleTypes.Add(vs.type, name);
                _vehicleSpecs.Add(name, vs);
                //if (vs.range < MIN_DRONE_RANGE) MIN_DRONE_RANGE = vs.range;
                vs.range = Mathf.Infinity;
            }
        }

        /// <summary>
        /// Coroutine for instantiation of background drones. @Eunu please comment on when we use this versus "AddBackgroundDrone()"
        /// </summary>
        public IEnumerator InstantiateBackgroundDrones(SceneManagerBase sceneManager, int backgroundDroneCount, float scale, float lowerElevationBound, float upperElevationBound, Canvas _canvas)
        {
            float progress = 0.0f;
            int vehiclesToInstantiate = backgroundDroneCount;
            string drone_path = vcs.IsRegionView ? "GUI/DroneIcon2" : "Drones/";
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            // Populate vehiclesToInstantiate number of drones in existing parking structures

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("Initializing Simulation");

            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated

                int vehicleTypeID = UnityEngine.Random.Range(0, _vehicleSpecs.Keys.Count);
                string path = vcs.IsRegionView ? drone_path : drone_path + _vehicleSpecs.Keys.ToList()[vehicleTypeID];
                var newDrone = Resources.Load<GameObject>(path);
                var type = _vehicleSpecs.Keys.ToList()[vehicleTypeID];

                float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
                Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
                // instantiate the vehicle at emptySpot
                var clone = InstantiateDrone(newDrone, instantiationSpot, out _);
                clone.name = "UAV_BACKGROUND_" + _backgroundDrones.Count.ToString();
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
                v.Initialize(_vehicleSpecs[type].type, _vehicleSpecs[type].capacity, _vehicleSpecs[type].range, _vehicleSpecs[type].maxSpeed, _vehicleSpecs[type].yawSpeed, _vehicleSpecs[type].takeoffSpeed, _vehicleSpecs[type].landingSpeed, _vehicleSpecs[type].emission, _vehicleSpecs[type].noise, "background");
                //v.currentPoint = v.gameObject.transform.posit;
                v.isBackgroundDrone = true;
                Vector3 firstDestination = vcs.GetRandomPointXZ(y);
                v.wayPointsQueue = new Queue<Vector3>(vcs.FindPath(instantiationSpot, firstDestination, 5, 1 << 8 | 1 << 9 | 1 << 13));
                v.targetPosition = v.wayPointsQueue.Dequeue();
                _droneTypeLookup.Add(clone, DroneType.Background);
                _backgroundDrones.Add(clone);

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
            string drone_path = vcs.IsRegionView ? "GUI/DroneIcon2" : "Drones/";
            string droneType = isCorridor ? "corridor" : "LowAltitude";
            float progress = 0.0f;
            int parkingCapacity = vcs.GetParkingCapacity();
            int typeSpecificCapacity = vcs.GetParkingCapacity(droneType);
            int currentDroneCt = isCorridor ? _corridorDrones.Count : _lowAltitudeDrones.Count;
            int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - typeSpecificCapacity - 10 - currentDroneCt, parkingCapacity - typeSpecificCapacity - currentDroneCt);

            if (vehiclesToInstantiate < 0) yield break;

            List<string> typeNames = new List<string>();

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("Instantiating drones");

            if (isCorridor) corridorDroneInstantiationStarted = true;
            else lowAltitudeDroneInstantiationStarted = true;



            foreach (string type in _vehicleTypes.Keys)
            {
                if (type.Equals(droneType)) typeNames.Add(_vehicleTypes[type]);
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
                            GameObject clone = InstantiateDrone(newDrone, translatedSpot, out var clone2d);

                            clone.name = "UAV_" + (isCorridor ? _corridorDrones.Count.ToString() : _lowAltitudeDrones.Count.ToString());
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
                            v.Clone2d = clone2d;
                            v.Initialize(_vehicleSpecs[type].type, _vehicleSpecs[type].capacity, _vehicleSpecs[type].range, _vehicleSpecs[type].maxSpeed, _vehicleSpecs[type].yawSpeed, _vehicleSpecs[type].takeoffSpeed, _vehicleSpecs[type].landingSpeed, _vehicleSpecs[type].emission, _vehicleSpecs[type].noise, "idle");
                            v.currentCommunicationPoint = sPS.gameObject;
                            OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
                            // Update parking management info
                            if (!vcs.IsRegionView) clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                            else
                            {
                                var image = clone2d.GetComponent<Image>();
                                if (image != null) image.enabled = false;
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
                            if (isCorridor)
                            {
                                _droneTypeLookup.Add(clone, DroneType.Corridor);
                                _corridorDrones.Add(clone);
                            }
                            else
                            {
                                _droneTypeLookup.Add(clone, DroneType.LowAltitude);
                                _lowAltitudeDrones.Add(clone);
                            }
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

            int vehicleTypeID = UnityEngine.Random.Range(0, _vehicleSpecs.Keys.Count);
            string drone_path = vcs.IsRegionView ? "GUI/DroneIcon2" : "Drones/" + _vehicleSpecs.Keys.ToList()[vehicleTypeID];

            var newDrone = Resources.Load<GameObject>(drone_path);
            var type = _vehicleSpecs.Keys.ToList()[vehicleTypeID];

            float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
            Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
            // instantiate the vehicle at emptySpot
            GameObject clone = InstantiateDrone(newDrone, instantiationSpot, out _);
            clone.name = "UAV_BACKGROUND_" + _backgroundDrones.Count.ToString();
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
            v.Initialize(_vehicleSpecs[type].type, _vehicleSpecs[type].capacity, _vehicleSpecs[type].range, _vehicleSpecs[type].maxSpeed, _vehicleSpecs[type].yawSpeed, _vehicleSpecs[type].takeoffSpeed, _vehicleSpecs[type].landingSpeed, _vehicleSpecs[type].emission, _vehicleSpecs[type].noise, "background");
            //v.currentPoint = v.gameObject.transform.posit;
            v.isBackgroundDrone = true;
            Vector3 firstDestination = vcs.GetRandomPointXZ(y);
            v.wayPointsQueue = new Queue<Vector3>(vcs.FindPath(instantiationSpot, firstDestination, 5, 1 << 8 | 1 << 9 | 1 << 13));
            v.targetPosition = v.wayPointsQueue.Dequeue();
            _droneTypeLookup.Add(clone, DroneType.Background);
            _backgroundDrones.Add(clone);
        }

        /// <summary>
        /// Unregisters drone from simulation.
        /// </summary>
        public void UnregisterDrone(GameObject drone)
        {
            if (!_droneTypeLookup.ContainsKey(drone)) return;
            var t = _droneTypeLookup[drone];
            switch(t)
            {
                case DroneType.Background:
                    {
                        _backgroundDrones.Remove(drone);
                        break;
                    }
                case DroneType.Corridor:
                    {
                        _corridorDrones.Remove(drone);
                        break;
                    }
                case DroneType.LowAltitude:
                    {
                        _lowAltitudeDrones.Remove(drone);
                        break;
                    }
            }
        }

        /// <summary>
        /// Instantiates any kind of drone. For region view, also makes sprite object, if applicable
        /// </summary>
        private GameObject InstantiateDrone(GameObject prefab, Vector3 spot, out GameObject clone2d)
        {
            GameObject clone;
            clone2d = null;
            if (vcs.IsRegionView)
            {
                clone = Instantiate(new GameObject(), spot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                //element follower knows to destroy itself if drone is destroyed.
                clone2d = Instantiate(prefab, vcs._canvas.transform);
                var f = clone2d.GetComponent<ElementFollower>();
                f?.Initialize(clone);
            }
            else
            {
                clone = Instantiate(prefab, spot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            }
            return clone;
        }
    }

}
