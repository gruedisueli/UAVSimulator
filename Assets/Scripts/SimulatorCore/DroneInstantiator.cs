using System;
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

namespace Assets.Scripts.SimulatorCore
{
    // Drone Infos
    
    public class DroneInstantiator: MonoBehaviour
    {

        private VehicleControlSystem vcs;
        private Dictionary<string, VehicleSpec> vehicleSpecs;
        private Dictionary<string, string> vehicleTypes;
        private string asset_root = "Assets\\";

        public event EventHandler<DroneInstantiationArgs> OnDroneInstantiated;
        

        public DroneInstantiator(VehicleControlSystem vcs)
        {
            vehicleSpecs = new Dictionary<string, VehicleSpec>();
            vehicleTypes = new Dictionary<string, string>();
            this.vcs = vcs;
            ReadVehicleSpecs();
        }
        public List<GameObject> InstantiateCorridorDrones(CityViewManager sceneManager, float scale)
        {
            int parkingCapacity = sceneManager.GetParkingCapacity();
            int lowAltitudeOnlyCapacity = sceneManager.GetParkingCapacity("LowAltitude");
            int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - lowAltitudeOnlyCapacity - 10, parkingCapacity - lowAltitudeOnlyCapacity);
            string drone_path = "Drones/";
            List<GameObject> vehicles = new List<GameObject>();
            List<string> corridorTypeNames = new List<string>();

            foreach ( string type in vehicleTypes.Keys )
            {
                if (type.Equals("corridor")) corridorTypeNames.Add(vehicleTypes[type]);
            }

            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                foreach (var key in sceneManager.ParkingStructures.Keys)
                {
                    var sPS = sceneManager.ParkingStructures[key];
                    ParkingControl pC = sPS.ParkingCtrl;
                    if (!sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
                    {
                        if (pC.parkingInfo.Parked.Count == 0) pC.parkingInfo.RemainingSpots = pC.parkingInfo.ParkingSpots.Count - pC.parkingInfo.Parked.Count;
                        if (pC.parkingInfo.RemainingSpots > 0)
                        {
                            int vehicleTypeID = UnityEngine.Random.Range(0, corridorTypeNames.Count);
                            var newDrone = Resources.Load<GameObject>(drone_path + corridorTypeNames[vehicleTypeID]);
                            var type = corridorTypeNames[vehicleTypeID];
                            var emptySpot = pC.parkingInfo.GetEmptySpot();
                            var translatedSpot = pC.parkingInfo.TranslateParkingSpot(emptySpot);

                            // instantiate the vehicle at emptySpot
                            var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                            clone.name = "UAV_" + i.ToString();
                            clone.tag = "Vehicle";
                            clone.layer = 10;
                            // Only for video
                            clone.transform.localScale = new Vector3(scale, scale, scale);
                            // ~Only for video
                            clone.AddComponent<VehicleNoise>();
                            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                            tr.material = Resources.Load<Material>("Materials/TrailCorridorDrones");
                            tr.time = Mathf.Infinity;
                            tr.enabled = false;

                            var sc = clone.AddComponent<SphereCollider>();
                            sc.radius = 1.0f;
                            sc.center = Vector3.zero;


                            UnityEngine.Object.Destroy(newDrone);

                            // Fill in vehivle spec
                            CorridorDrone v = clone.AddComponent<CorridorDrone>();
                            v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "idle");
                            v.currentCommunicationPoint = sPS.gameObject;
                            vehicles.Add(clone);
                            OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
                            // Update parking management info
                            clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                            pC.parkingInfo.ParkAt(emptySpot, clone);

                            break;
                        }
                    }
                }

            }
            return vehicles;
        }

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
        public List<GameObject> InstantiateLowAltitudeDrones(CityViewManager sceneManager, float scale)
        {
            List<GameObject> vehicles = new List<GameObject>();
            int parkingCapacity = sceneManager.GetParkingCapacity();
            int vehiclesToInstantiate = UnityEngine.Random.Range(parkingCapacity - 10, parkingCapacity);
            string drone_path = "Drones/";

            List<string> lowAltitudeTypeNames = new List<string>();

            foreach (string type in vehicleTypes.Keys)
            {
                if (type.Equals("LowAltitude")) lowAltitudeTypeNames.Add(vehicleTypes[type]);
            }

            // Populate vehiclesToInstantiate number of drones in existing parking structures
            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                foreach (var key in sceneManager.ParkingStructures.Keys)
                {
                    var sPS = sceneManager.ParkingStructures[key];
                    ParkingControl pC = sPS.ParkingCtrl;
                    if (sPS.ParkingStructureSpecs.Type.Contains("LowAltitude"))
                    {
                        if (pC.parkingInfo.Parked.Count == 0) pC.parkingInfo.RemainingSpots = pC.parkingInfo.ParkingSpots.Count - pC.parkingInfo.Parked.Count;
                        if (pC.parkingInfo.RemainingSpots > 0)
                        {
                            int vehicleTypeID = UnityEngine.Random.Range(0, lowAltitudeTypeNames.Count);
                            var newDrone = Resources.Load<GameObject>(drone_path + lowAltitudeTypeNames[vehicleTypeID]);
                            var type = lowAltitudeTypeNames[vehicleTypeID];
                            var emptySpot = pC.parkingInfo.GetEmptySpot();
                            var translatedSpot = pC.parkingInfo.TranslateParkingSpot(emptySpot);

                            // instantiate the vehicle at emptySpot
                            var clone = Instantiate(newDrone, translatedSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                            clone.name = "UAV_" + i.ToString();
                            clone.tag = "Vehicle";
                            clone.layer = 10;
                            // Only for video
                            clone.transform.localScale = new Vector3(scale, scale, scale);
                            // ~Only for video
                            clone.AddComponent<VehicleNoise>();
                            TrailRenderer tr = clone.AddComponent<TrailRenderer>();
                            tr.material = Resources.Load<Material>("Materials/TrailLowAltitudeDrone");
                            tr.time = Mathf.Infinity;
                            tr.enabled = false;

                            var sc = clone.AddComponent<SphereCollider>();
                            sc.radius = 1.0f;
                            sc.center = Vector3.zero;


                            UnityEngine.Object.Destroy(newDrone);

                            // Fill in vehivle spec
                            LowAltitudeDrone v = clone.AddComponent<LowAltitudeDrone>();
                            v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "idle");
                            v.currentCommunicationPoint = sPS.gameObject;
                            v.approaching_threshold = 100.0f;
                            vehicles.Add(clone);
                            OnDroneInstantiated?.Invoke(this, new DroneInstantiationArgs(clone));
                            // Update parking management info
                            clone.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                            
                            pC.parkingInfo.ParkAt(emptySpot, clone);

                            break;
                        }
                    }
                }
            }
            return vehicles;
        }

        public List<GameObject> InstantiateBackgroundDrones(CityViewManager sceneManager, int backgroundDroneCount, float scale, float lowerElevationBound, float upperElevationBound)
        {
            List<GameObject> backgroundDrones = new List<GameObject>();
            int vehiclesToInstantiate = backgroundDroneCount;
            string drone_path = "Drones/";
            Material backgroundTrail = Resources.Load<Material>("Materials/TrailBackGroundDrones");
            // Populate vehiclesToInstantiate number of drones in existing parking structures
            for (int i = 0; i < vehiclesToInstantiate; i++)
            {
                // INTEGRATION TO-DO: Make this part to select parking structure randomly so that the drones are randomly populated

                int vehicleTypeID = UnityEngine.Random.Range(0, vehicleSpecs.Keys.Count);
                var newDrone = Resources.Load<GameObject>(drone_path + vehicleSpecs.Keys.ToList()[vehicleTypeID]);
                var type = vehicleSpecs.Keys.ToList()[vehicleTypeID];

                float y = UnityEngine.Random.Range(lowerElevationBound, upperElevationBound);
                Vector3 instantiationSpot = vcs.GetRandomPointXZ(y);
                // instantiate the vehicle at emptySpot
                var clone = Instantiate(newDrone, instantiationSpot, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                clone.name = "UAV_BACKGROUND_" + i.ToString();
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
                UnityEngine.Object.Destroy(newDrone);

                // Fill in vehivle spec
                BackgroundDrone v = clone.AddComponent<BackgroundDrone>();
                v.Initialize(vehicleSpecs[type].type, vehicleSpecs[type].capacity, vehicleSpecs[type].range, vehicleSpecs[type].maxSpeed, vehicleSpecs[type].yawSpeed, vehicleSpecs[type].takeoffSpeed, vehicleSpecs[type].landingSpeed, vehicleSpecs[type].emission, vehicleSpecs[type].noise, "background");
                //v.currentPoint = v.gameObject.transform.posit;
                v.isBackgroundDrone = true;
                Vector3 firstDestination = vcs.GetRandomPointXZ(y);
                v.wayPointsQueue = new Queue<Vector3>(sceneManager.FindPath(instantiationSpot, firstDestination, 5));
                v.targetPosition = v.wayPointsQueue.Dequeue();
                backgroundDrones.Add(clone);

                // Update parking management info
            }
            return backgroundDrones;
        }
    }

}
