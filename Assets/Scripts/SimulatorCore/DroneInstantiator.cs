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

        public bool isCorridorDroneInstantiated;
        public bool isLowAltitudeDroneInstantiated;
        public bool isBackgroundDroneInstantiated;

        public bool corridorDroneInstantiationStarted;
        public bool lowAltitudeDroneInstantiationStarted;

        public Dictionary<GameObject, DroneType> _droneTypeLookup = new Dictionary<GameObject, DroneType>();
        public List<GameObject> _backgroundDrones = new List<GameObject>();
        public List<GameObject> _corridorDrones = new List<GameObject>();
        public List<GameObject> _lowAltitudeDrones = new List<GameObject>();

        private Coroutine _corridorDrnCoroutine, _lowAltDrnRoutine, _backgroundDrnRoutine;
        private List<GameObject> _progressBars = new List<GameObject>();

        public event EventHandler<DroneInstantiationArgs> OnDroneInstantiated;

        public void Init(VehicleControlSystem vcs)
        {
            this.vcs = vcs;
            ReadVehicleSpecs();
        }

        /// <summary>
        /// Clears out the simulation of drones.
        /// </summary>
        public void ClearDrones()
        {
            if (_lowAltDrnRoutine != null)
            {
                StopCoroutine(_lowAltDrnRoutine);
            }

            if (_corridorDrnCoroutine != null)
            {
                StopCoroutine(_corridorDrnCoroutine);
            }

            if (_backgroundDrnRoutine != null)
            {
                StopCoroutine(_backgroundDrnRoutine);
            }

            for (int i = 0; i < _progressBars.Count; i++)
            {
                _progressBars[i].Destroy();
            }
            _progressBars.Clear();

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
            _backgroundDrones.Clear();
            _corridorDrones.Clear();
            _lowAltitudeDrones.Clear();

            foreach (var pS in vcs.sceneManager.ParkingStructures.Values)
            {
                pS.ParkingCtrl.ResetSimulation();
            }

            foreach (var dP in vcs.sceneManager.DronePorts.Values)
            {
                dP.DronePortCtrl.ResetSimulation();
            }
            isBackgroundDroneInstantiated = false;
        }

        /// <summary>
        /// Main method called for instantiating drones in the simulation.
        /// </summary>
        public void InstantiateDrones(SceneManagerBase sceneManager, float scale, Canvas _canvas)
        {
            _corridorDrnCoroutine = StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, true));
            _lowAltDrnRoutine = StartCoroutine(InstantiateCorridorOrLowAltDrones(sceneManager, scale, _canvas, false));
        }

        /// <summary>
        /// Parses vehicle specs from external file.
        /// </summary>
        public void ReadVehicleSpecs()
        {
            if (_vehicleTypes.Keys.Count > 0) return;
            string rPath = "Drones/";
            var textAssets = Resources.LoadAll<TextAsset>(rPath);
            if (textAssets == null)
            {
                Debug.LogError("Could not locate drone text assets");
                return;
            }
            foreach (var j in textAssets)
            {
                VehicleSpec vS = JsonUtility.FromJson<VehicleSpec>(j.text);
                _vehicleTypes.Add(vS.type, j.name);
                _vehicleSpecs.Add(j.name, vS);
                //if (vs.range < MIN_DRONE_RANGE) MIN_DRONE_RANGE = vs.range;
                vS.range = Mathf.Infinity;
            }
        }

        /// <summary>
        /// Instantiates all new background drones.
        /// </summary>
        public void InstantiateBackgroundDrones(SceneManagerBase sceneManager, int backgroundDroneCount, float scale, float lowerElevationBound, float upperElevationBound, Canvas _canvas)
        {
            _backgroundDrnRoutine = StartCoroutine(InstantiateBackgroundDronesCoroutine(sceneManager, backgroundDroneCount, scale, lowerElevationBound, upperElevationBound, _canvas));
        }

        /// <summary>
        /// Coroutine for instantiation of background drones. @Eunu please comment on when we use this versus "AddBackgroundDrone()"
        /// </summary>
        private IEnumerator InstantiateBackgroundDronesCoroutine(SceneManagerBase sceneManager, int backgroundDroneCount, float scale, float lowerElevationBound, float upperElevationBound, Canvas _canvas)
        {
            float progress = 0.0f;
            int vehiclesToInstantiate = backgroundDroneCount;
            string drone_path = vcs.TEMPORARY_IsRegionView ? "GUI/DroneIcon2" : "Drones/";
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            // Populate vehiclesToInstantiate number of drones in existing parking structures

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            _progressBars.Add(pG);
            progressBar.Init("Initializing Simulation");

            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated

                int vehicleTypeID = UnityEngine.Random.Range(0, _vehicleSpecs.Keys.Count);
                string path = vcs.TEMPORARY_IsRegionView ? drone_path : drone_path + _vehicleSpecs.Keys.ToList()[vehicleTypeID];
                var newDrone = Resources.Load<GameObject>(path);
                var type = _vehicleSpecs.Keys.ToList()[vehicleTypeID];

                float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
                Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
                // instantiate the vehicle at emptySpot
                var clone = InstantiateDrone(newDrone, instantiationSpot, out var clone2d);
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
                v.Clone2d = clone2d;
                _droneTypeLookup.Add(clone, DroneType.Background);
                _backgroundDrones.Add(clone);

                // Update parking management info

                progress = (float)i / (float)vehiclesToInstantiate;
                progressBar.SetCompletion(progress);
                yield return null;
            }

            _progressBars.Remove(pG);
            pG.Destroy();
            isBackgroundDroneInstantiated = true;
        }

        /// <summary>
        /// Main coroutine for instantiating drones of low altitude or corridor type.
        /// </summary>
        private IEnumerator InstantiateCorridorOrLowAltDrones(SceneManagerBase sceneManager, float scale, Canvas _canvas, bool isCorridor)
        {
            string drone_path = vcs.TEMPORARY_IsRegionView ? "GUI/DroneIcon2" : "Drones/";
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
            _progressBars.Add(pG);
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
                            string path = vcs.TEMPORARY_IsRegionView ? drone_path : drone_path + typeNames[vehicleTypeID];
                            var newDrone = Resources.Load<GameObject>(path);
                            var type = typeNames[vehicleTypeID];
                            var emptySpot = pC.GetEmptySpot();
                            var translatedSpot = pC.TranslateParkingSpot(emptySpot);

                            // instantiate the vehicle at emptySpot
                            GameObject clone = InstantiateDrone(newDrone, translatedSpot, out var clone2d);

                            clone.name = "UAV_" + (isCorridor ? "Corridor_" + _corridorDrones.Count : "Low Altitude_" + _lowAltitudeDrones.Count);
                            clone.tag = "Vehicle";
                            clone.layer = 10;
                            // Only for video
                            clone.transform.localScale = new Vector3(scale, scale, scale);
                            // ~Only for video
                            
                            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                            tr.material = Resources.Load<Material>("Materials/TrailCorridorDrones");
                            tr.time = Mathf.Infinity;
                            tr.enabled = false;

                            // Fill in vehicle spec
                            DroneBase v = isCorridor ? (DroneBase)clone.AddComponent<CorridorDrone>() : (DroneBase)clone.AddComponent<LowAltitudeDrone>();
                            var vN = clone.AddComponent<VehicleNoise>();
                            vN.Init(vcs);
                            v.Clone2d = clone2d;
                            v.Initialize(_vehicleSpecs[type].type, _vehicleSpecs[type].capacity, _vehicleSpecs[type].range, _vehicleSpecs[type].maxSpeed, _vehicleSpecs[type].yawSpeed, _vehicleSpecs[type].takeoffSpeed, _vehicleSpecs[type].landingSpeed, _vehicleSpecs[type].emission, _vehicleSpecs[type].noise, "idle");
                            v.currentCommunicationPoint = sPS.gameObject;

                            var cloneI = Instantiate(EnvironManager.Instance.DroneIconPrefab);
                            cloneI.transform.SetParent(_canvas.transform, false);
                            var icon = cloneI.GetComponentInChildren<DroneIcon>(true);
                            if (icon == null)
                            {
                                Debug.LogError("Could not find Drone Icon Component in Drone Info Panel prefab");
                                continue;
                            }
                            icon.Initialize(v);
                            v.SelectionCircle = cloneI;

                            OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(icon));
                            // Update parking management info
                            if (!vcs.TEMPORARY_IsRegionView) clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
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
            _progressBars.Remove(pG);
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
            string drone_path = vcs.TEMPORARY_IsRegionView ? "GUI/DroneIcon2" : "Drones/" + _vehicleSpecs.Keys.ToList()[vehicleTypeID];

            var newDrone = Resources.Load<GameObject>(drone_path);
            var type = _vehicleSpecs.Keys.ToList()[vehicleTypeID];

            float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
            Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
            // instantiate the vehicle at emptySpot
            GameObject clone = InstantiateDrone(newDrone, instantiationSpot, out var clone2d);
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
            v.Clone2d = clone2d;
            _droneTypeLookup.Add(clone, DroneType.Background);
            _backgroundDrones.Add(clone);
        }

        /// <summary>
        /// Sets number of background drones in simulation.
        /// </summary>
        public void SetBackgroundDroneCt(int count)
        {
            SimulationAnalyzer sa = gameObject.GetComponent<SimulationAnalyzer>();
            if (count < _backgroundDrones.Count + sa.flyingDrones.Count)//if too many drones are in simulation, we need to remove some.
            {
                int dronesToRemove = _backgroundDrones.Count - count;
                int removed = 0;
                if (_backgroundDrones.Count > 0)
                {
                    for (int i = 0; i < dronesToRemove && _backgroundDrones.Count > 0; i++)
                    {
                        var droneToDestroy = _backgroundDrones.Last();
                        _backgroundDrones.Remove(droneToDestroy);

                        if (_droneTypeLookup.ContainsKey(droneToDestroy))
                        {
                            _droneTypeLookup.Remove(droneToDestroy);
                        }
                        else
                        {
                            Debug.LogError("Drone to destroy not found in type lookup dictionary");
                        }
                        droneToDestroy.Destroy();
                        removed++;
                    }
                }
            }
            else if (count == _backgroundDrones.Count)//no reason to change anything.
            {
                return;
            }
            else//we should add some
            {
                int dronesToAdd = count - _backgroundDrones.Count;
                for (int i = 0; i < dronesToAdd; i++)
                {
                    AddBackgroundDrone(vcs.sceneManager, vcs.scaleMultiplier, vcs.lowerElevationBound, vcs.upperElevationBound);
                }
            }

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
            if (vcs.TEMPORARY_IsRegionView)
            {
                clone = new GameObject();
                clone.transform.position = spot;
                //clone = Instantiate(new GameObject(), spot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                //element follower knows to destroy itself if drone is destroyed.
                clone2d = Instantiate(prefab, vcs._canvas.transform);
                clone2d.name = "Drone sprite";
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
