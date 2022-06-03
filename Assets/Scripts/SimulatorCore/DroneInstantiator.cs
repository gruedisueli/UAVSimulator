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
using Random = System.Random;

namespace Assets.Scripts.SimulatorCore
{
    /// <summary>
    /// Manages the actual instantiation of drone game objects in the simulation.
    /// </summary>
    public class DroneInstantiator: MonoBehaviour
    {

        private VehicleControlSystem vcs;
        private Random _random = new Random();

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
            

            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            _progressBars.Add(pG);
            progressBar.Init("Initializing Simulation");
          
            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                AddBackgroundDrone();

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
            var droneSettings = isCorridor ? EnvironManager.Instance.Environ.SimSettings.CorridorDroneSettings : EnvironManager.Instance.Environ.SimSettings.LowAltitudeDroneSettings;
            //string droneType = isCorridor ? "corridor" : "LowAltitude";
            
            //int parkingCapacity = vcs.GetParkingCapacity();
            //int typeSpecificCapacity = vcs.GetParkingCapacity(droneSettings.DroneType);
            //int currentDroneCt = isCorridor ? _corridorDrones.Count : _lowAltitudeDrones.Count;
            //int vehiclesToInstantiate = UnityEngine.Random.Range(typeSpecificCapacity - 10 - currentDroneCt, typeSpecificCapacity - 10);
            //int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - typeSpecificCapacity - 10 - currentDroneCt, parkingCapacity - typeSpecificCapacity - currentDroneCt);

            //if (vehiclesToInstantiate < 0) yield break;

            if (isCorridor) corridorDroneInstantiationStarted = true;
            else lowAltitudeDroneInstantiationStarted = true;

            foreach (var key in sceneManager.ParkingStructures.Keys)
            {
                var sPS = sceneManager.ParkingStructures[key];
                ParkingControl pC = sPS.ParkingCtrl;
                bool isLE = sPS.ParkingStructureSpecs.Type.Contains("LowAltitude");
                if ((isCorridor && isLE) || (!isCorridor && !isLE))
                {
                    continue;
                }
                float progress = 0.0f;
                var ct = sPS.ParkingStructureSpecs.GetDroneInstantiationCt();
                var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
                var progressBar = pG.GetComponent<ProgressBar>();
                progressBar.Init("Instantiating drones");
                _progressBars.Add(pG);
                for (int c = 0; c < ct && pC.RemainingSpots > 0; c++)
                {
                    if (pC.RemainingSpots > 0)
                    {
                        string path = vcs.TEMPORARY_IsRegionView ? drone_path : drone_path + droneSettings.DroneType;
                        var newDrone = Resources.Load<GameObject>(path);
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
                        v.Clone2d = clone2d;
                        v.Initialize(ref droneSettings, "idle", false);
                        v.CurrentCommunicationPoint = sPS.gameObject;

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
                    }
                    
                    progress = (float)c / (float)ct;
                    progressBar.SetCompletion(progress);
                    yield return null;

                }
                _progressBars.Remove(pG);
                pG.Destroy();
            }
            
            //return vehicles;
            if (isCorridor) isCorridorDroneInstantiated = true;
            else isLowAltitudeDroneInstantiated = true;
        }

        public void AddBackgroundDrone()
        {
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            string drone_path = vcs.TEMPORARY_IsRegionView ? "GUI/DroneIcon2" : "Drones/";
            var simSettings = EnvironManager.Instance.Environ.SimSettings;

            var isCorridor = _random.Next(2) == 1;
            var droneSettings = isCorridor ? simSettings.CorridorDroneSettings : simSettings.LowAltitudeDroneSettings;

            string path = vcs.TEMPORARY_IsRegionView ? drone_path : drone_path + droneSettings.DroneType;
            var newDrone = Resources.Load<GameObject>(path);

            float y = UnityEngine.Random.Range(simSettings.BackgoundDroneLowerElev_M, simSettings.BackgroundDroneUpperElev_M);
            Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
            // instantiate the vehicle at emptySpot
            var clone = InstantiateDrone(newDrone, instantiationSpot, out var clone2d);
            clone.name = "UAV_BACKGROUND_" + _backgroundDrones.Count.ToString();
            clone.tag = "Vehicle";
            clone.layer = 10;

            clone.transform.localScale = new Vector3(vcs.scaleMultiplier, vcs.scaleMultiplier, vcs.scaleMultiplier);
            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
            tr.startColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.endColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            tr.material = backgroundTrail;
            tr.time = 60.0f;

            // Fill in vehivle spec
            BackgroundDrone v = clone.AddComponent<BackgroundDrone>();
            v.Initialize(ref droneSettings, "background", true);
            Vector3 firstDestination = vcs.GetRandomPointXZ(y);
            v.WayPointsQueue = new Queue<Vector3>(vcs.FindPath(instantiationSpot, firstDestination, 5, 1 << 8 | 1 << 9 | 1 << 13));
            v.TargetPosition = v.WayPointsQueue.Dequeue();
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
                    AddBackgroundDrone();
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
