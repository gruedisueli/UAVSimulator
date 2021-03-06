using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using Mapbox.Unity.Map;

using Assets.Scripts.Environment;
using Assets.Scripts.MapboxCustom;
using Assets.Scripts.UI.Panels;

namespace Assets.Scripts.UI
{
    public class CityViewManager : SceneManagerBase
    {
        public AbstractMap abstractMap;
        public FOA_RangeAroundTransformTileProvider _tileProvider;
        public Material selectedMaterial;
        public GameObject _cityCenter;
        public Camera _mainCamera;
        public SavePrompt _savePrompt;

        private List<Tuple<GameObject, Material, Material[], Vector3>> selectedObjects = new List<Tuple<GameObject, Material, Material[], Vector3>>();

        protected override void Init()
        {
            var eC = EnvironManager.Instance;
            //var city = eC.Environ.GetCity(eC.ActiveCity);
            var city = eC.GetCurrentCity();
            if (city != null)
            {
                _cityCenter.transform.position = city.WorldPos;
                var options = new FOA_RangeAroundTransformTileProviderOptions();
                options._eastExt = city.CityStats.EastExt;
                options._westExt = city.CityStats.WestExt;
                options._northExt = city.CityStats.NorthExt;
                options._southExt = city.CityStats.SouthExt;
                options._targetTransform = _cityCenter.transform;
                _tileProvider.SetOptions(options);
                _mainCamera.transform.position = new Vector3(city.WorldPos.x, 1000, city.WorldPos.z);
                abstractMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.CITY_ZOOM_LEVEL);
                InstantiateObjects();
            }
        }

        /// <summary>
        /// Loads up the region view of current environment
        /// </summary>
        public void GoToRegion()
        {
            SceneManager.LoadScene(UISettings.REGIONVIEW_SCENEPATH, LoadSceneMode.Single);
        }

        /// <summary>
        /// Initiates process of going to main menu
        /// </summary>
        public void GoMain()
        {
            _savePrompt.GoMain();
        }

        /// <summary>
        /// Initiates process of quitting
        /// </summary>
        public void Quit()
        {
            _savePrompt.Quit();
        }

        private void Deselect()
        {
            for (int s = selectedObjects.Count - 1; s >= 0; s--)
            {
                var sO = selectedObjects[s];
                var mR = sO.Item1.GetComponent<MeshRenderer>();
                if (mR != null)
                {
                    mR.sharedMaterial = sO.Item2;
                    mR.sharedMaterials = sO.Item3;
                }
                selectedObjects.RemoveAt(s);
            }
        }

        private void Select()
        {

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hit)
            {
                var gO = hitInfo.transform.gameObject;
                Debug.Log("Hit " + gO.name);
                //if (gO.name.Contains("Buildings"))
                //{
                //    Debug.Log("Building Selected");
                //}
                //else
                //{
                //    Debug.Log("Not a building");
                //}
                var mR = gO.GetComponent<MeshRenderer>();
                Material mainMat = mR.sharedMaterial;
                Material[] mats = mR != null ? mR.sharedMaterials : new Material[0];
                var p = hitInfo.point;
                selectedObjects.Add(Tuple.Create(gO, mainMat, mats, p));
                if (mR != null)
                {
                    mR.sharedMaterial = selectedMaterial;
                    mR.sharedMaterials = new Material[] { selectedMaterial, selectedMaterial };
                }
            }
            else
            {
                Debug.Log("No hit");
            }

        }

        /// <summary>
        /// Adds a new drone port from UI.
        /// </summary>
        private void AddNewDronePort(DronePortBase dP)
        {
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddDronePort(guid, dP);
            InstantiateDronePort(guid, dP);
        }

        /// <summary>
        /// Adds a new parking structure from UI.
        /// </summary>
        private void AddNewParkingStruct(ParkingStructureBase pS)
        {
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddParkingStructure(guid, pS);
            InstantiateParkingStructure(guid, pS);
        }

        /// <summary>
        /// Adds a new restriction zone from UI.
        /// </summary>
        private void AddNewRestrictZone(RestrictionZoneBase rZ)
        {
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddRestrictionZone(guid, rZ);
            InstantiateRestrictionZone(guid, rZ);
        }

        ///// <summary>
        ///// Modifies a drone port with provided parameters
        ///// </summary>
        //private void ModifyDronePort(string guid, DronePortBase dP)
        //{
        //    EnvironManager.Instance.SetDronePort(guid, dP);
        //    if (_dronePorts.ContainsKey(guid))
        //    {
        //        _dronePorts[guid].SceneGameObject.Destroy();
        //        _dronePorts.Remove(guid);
        //        InstantiateDronePort(guid, dP);
        //    }
        //    else
        //    {
        //        Debug.LogError("Specified drone port not in dictionary");
        //    }

        //}

        ///// <summary>
        ///// Instantiates a new custom drone port in the project from assets. Does not update environment.
        ///// </summary>
        //private void InstantiateCustomDronePort(DronePortAssetPack asset)
        //{
        //    InstantiateCustomDronePort(asset.Prefab, asset.Specs);
        //}

        ///// <summary>
        ///// Instantiates a new custom parking structure in the project from assets. Does not update environment.
        ///// </summary>
        //private void InstantiateCustomParkingStruct(ParkingStructureAssetPack asset)
        //{
        //    InstantiateCustomParkingStruct(asset.Prefab, asset.Specs);
        //}

        /// <summary>
        /// Instantiates a drone port. Does not update enviornment.
        /// </summary>
        private void InstantiateDronePort(string guid, DronePortBase dP)
        {
            if (dP is DronePortRect)
            {
                InstantiateRectDronePort(guid, dP as DronePortRect);
            }
            else if (dP is DronePortCustom)
            {
                var pfb = EnvironManager.Instance.DronePortAssets[dP.Type].Prefab;
                InstantiateCustomDronePort(guid, pfb, dP as DronePortCustom);
            }
            else
            {
                Debug.LogError("Drone port type requested for instantiation not recognized");
            }
        }

        /// <summary>
        /// Instantiates a parking structure. Does not update enviornment.
        /// </summary>
        private void InstantiateParkingStructure(string guid, ParkingStructureBase pS)
        {
            if (pS is ParkingStructureRect)
            {
                InstantiateRectParkingStruct(guid, pS as ParkingStructureRect);
            }
            else if (pS is ParkingStructureCustom)
            {
                var pfb = EnvironManager.Instance.ParkingStructAssets[pS.Type].Prefab;
                InstantiateCustomParkingStruct(guid, pfb, pS as ParkingStructureCustom);
            }
            else
            {
                Debug.LogError("Parking structure type requested for instantiation not recognized");
            }
        }

        /// <summary>
        /// Instantiates a restriction zone. Does not update enviornment.
        /// </summary>
        private void InstantiateRestrictionZone(string guid, RestrictionZoneBase rZ)
        {
            if (rZ is RestrictionZoneRect)
            {
                InstantiateRectRestrictZone(guid, rZ as RestrictionZoneRect);
            }
            else if (rZ is RestrictionZoneCyl)
            {
                InstantiateCylRestrictZone(guid, rZ as RestrictionZoneCyl);
            }
            else if (rZ is RestrictionZoneCylStack)
            {
                InstantiateStackedRestrictZone(guid, rZ as RestrictionZoneCylStack);
            }
            else
            {
                Debug.LogError("Restriction zone type requested for instantiation not recognized");
            }
        }

        /// <summary>
        /// Instantiates a generic rectangular drone port in project. Does not update environment.
        /// </summary>
        private void InstantiateRectDronePort(string guid, DronePortRect dP)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);

            clone.name = "DronePort_" + dP.Type;
            clone.tag = "DronePort";
            clone.layer = dP.Layer;
            clone.transform.position = dP.Position;
            clone.transform.rotation = Quaternion.Euler(dP.Rotation.x, dP.Rotation.y, dP.Rotation.z);
            clone.transform.localScale = dP.Scale;
            DronePortControl newDronePort = clone.AddComponent<DronePortControl>();
            newDronePort.dronePortInfo = dP;

            _dronePorts.Add(guid, new SceneDronePort(clone, dP));
        }

        /// <summary>
        /// Instantiates a custom drone port in the project. Does not update environment.
        /// </summary>
        private void InstantiateCustomDronePort(string guid, GameObject prefab, DronePortCustom dP)
        {
            var clone = Instantiate(prefab, dP.Position, Quaternion.Euler(dP.Rotation.x, dP.Rotation.y, dP.Rotation.z));

            clone.name = "DronePort_" + dP.Type;
            clone.tag = "DronePort";
            clone.layer = dP.Layer;
            clone.transform.localScale = dP.Scale;
            DronePortControl newDronePort = clone.AddComponent<DronePortControl>();
            newDronePort.dronePortInfo = dP;

            _dronePorts.Add(guid, new SceneDronePort(clone, dP));
        }

        /// <summary>
        /// Instantiates a custom parking structure in the project. Does not update environment.
        /// </summary>
        private void InstantiateCustomParkingStruct(string guid, GameObject prefab, ParkingStructureCustom pS)
        {
            var clone = Instantiate(prefab, pS.Position, Quaternion.Euler(pS.Rotation.x, pS.Rotation.y, pS.Rotation.z));

            clone.tag = "ParkingStructure";
            clone.name = "Parking_" + pS.Type;
            clone.layer = pS.Layer;
            clone.transform.localScale = pS.Scale;

            Parking newStructure = clone.AddComponent<Parking>();
            newStructure.parkingInfo = pS;

            _parkingStructures.Add(guid, new SceneParkingStructure(clone, pS));
        }


        /// <summary>
        /// Instantiates a rectangular parking structure in the project. Does not update environment.
        /// </summary>
        private void InstantiateRectParkingStruct(string guid, ParkingStructureRect pS)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);

            clone.tag = "ParkingStructure";
            clone.name = "Parking_" + pS.Type;
            clone.layer = pS.Layer;
            clone.transform.position = pS.Position;
            clone.transform.rotation = Quaternion.Euler(pS.Rotation.x, pS.Rotation.y, pS.Rotation.z);
            clone.transform.localScale = pS.Scale;

            Parking newStructure = clone.AddComponent<Parking>();
            newStructure.parkingInfo = pS;

            _parkingStructures.Add(guid, new SceneParkingStructure(clone, pS));
        }

        /// <summary>
        /// Instantiates a stacked restriction zone in the project. Does not update environment.
        /// </summary>
        private void InstantiateStackedRestrictZone(string guid, RestrictionZoneCylStack rZ)
        {
            foreach(var cyl in rZ.Elements)
            {
                InstantiateCylRestrictZone(guid, cyl);
            }
        }

        /// <summary>
        /// Instantiates a rectangular restriction zone in the project. Does not update environment.
        /// </summary>
        private void InstantiateRectRestrictZone(string guid, RestrictionZoneRect rZ)
        {
            GameObject clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clone.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height, rZ.Scale.z);
            clone.transform.position = new Vector3(rZ.Position.x, rZ.Height / 2, rZ.Position.z);
            clone.transform.rotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
            clone.layer = rZ.Layer;
            FinishRestrictZoneInst(guid, ref clone, rZ.Type);
        }

        /// <summary>
        /// Instantiates a cylindrical restriction zone in the project. Does not update environment.
        /// </summary>
        private void InstantiateCylRestrictZone(string guid, RestrictionZoneCyl rZ)
        {
            GameObject clone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            clone.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height, rZ.Scale.z);
            clone.transform.position = new Vector3(rZ.Position.x, rZ.Height / 2, rZ.Position.z);
            clone.transform.rotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
            clone.layer = rZ.Layer;
            FinishRestrictZoneInst(guid, ref clone, rZ.Type);
        }

        private void FinishRestrictZoneInst(string guid, ref GameObject clone, string type)
        {
            clone.name = "RestrictionZone_" + type;
            clone.tag = "RestrictionZone";
            var mR = clone.AddComponent<MeshRenderer>();
            mR.material = _restrictionZoneMaterial;

            _restrictionZones.Add(guid, clone);
        }

        public void InstantiateObjects()
        {
            // (Eunu? stll relevant?) TO-DO: Also register the restriction zones for drone ports and parking structures

            // Instantiate different types of drone ports in the correct locations
            var eM = EnvironManager.Instance;
            var city = eM.GetCurrentCity();
            foreach (var kvp in city.DronePorts)
            {
                var dP = kvp.Value;
                InstantiateDronePort(kvp.Key, dP);


                //if (dp.isCustom)
                //{
                //    // Instantiate drone port game object from obj stored in /assets/resources/droneports
                //    var newObject = Resources.Load<GameObject>(path + dp.type);
                //    var clone = Instantiate(newObject, dp.position, Quaternion.Euler(dp.rotation.x, dp.rotation.y, dp.rotation.z));
                //    clone.name = "DronePort_" + dp.type;
                //    clone.tag = "DronePort";
                //    clone.layer = 12;
                //    // Fill in type specific informations

                //    dp.maximumVehicleSize = eM.DronePortSpecs[dp.type].maximumVehicleSize;
                //    dp.isMountable = eM.DronePortSpecs[dp.type].isMountable;
                //    dp.isOnTheGround = eM.DronePortSpecs[dp.type].isOnTheGround;
                //    dp.isScalable = eM.DronePortSpecs[dp.type].isScalable;
                //    _dronePorts.Add(clone, dp);
                //    DronePortControl newDronePort = clone.AddComponent<DronePortControl>();
                //    newDronePort.dronePortInfo = dp;
                //    Destroy(newObject);

                //}
                //else
                //{
                //    if (dp.type.Contains("rectangular"))
                //    {
                //        // Instantiate drone port game object by creating primitive cube
                //        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //        cube.tag = "DronePort";
                //        cube.name = "DronePort_Rectangular";
                //        cube.layer = 12;
                //        cube.transform.position = dp.position;
                //        cube.transform.rotation = Quaternion.Euler(dp.rotation.x, dp.rotation.y, dp.rotation.z);
                //        cube.transform.localScale = dp.scale;
                //        // Fill in type specific information
                //        dp.isMountable = true;
                //        dp.isOnTheGround = true;
                //        dp.isScalable = true;


                //        // TO-DO: Update this to be the actual number
                //        dp.maximumVehicleSize = 0.5f * dp.scale.x;

                //        DronePortControl newDronePort = cube.AddComponent<DronePortControl>();
                //        newDronePort.dronePortInfo = dp;

                //        _dronePorts.Add(cube, dp);
                //    }
                //}
            }

            //path = "ParkingStructures/";
            foreach (var kvp in city.ParkingStructures)
            {
                var pS = kvp.Value;
                InstantiateParkingStructure(kvp.Key, pS);

                //if (ps.isCustom)
                //{
                //    // Instantiate parking structure from obj files stored in /assets/resources
                //    var newObject = Resources.Load<GameObject>(path + ps.type);
                //    var clone = Instantiate(newObject, ps.position, Quaternion.Euler(ps.rotation.x, ps.rotation.y, ps.rotation.z));
                //    clone.tag = "ParkingStructure";
                //    clone.name = "Parking_" + ps.type;
                //    clone.layer = 11;

                //    // Fill in type specific information
                //    ps.parkingSpots = new List<Vector3>(eM.ParkingStructSpecs[ps.type]);
                //    ps.remainingSpots = ps.parkingSpots.Count;
                //    _parkingStructures.Add(clone, ps);

                //    Parking newStructure = clone.AddComponent<Parking>();
                //    ps.parked = new Dictionary<Vector3, GameObject>();
                //    ps.vehicleAt = new Dictionary<GameObject, Vector3>();
                //    ps.reserved = new Dictionary<GameObject, Vector3>();
                //    newStructure.parkingInfo = ps;



                //    Destroy(newObject);
                //}
                //else
                //{
                //    if (ps.type.Contains("rectangular"))
                //    {
                //        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //        cube.tag = "ParkingStructure";
                //        cube.name = "Parking_Rectangular";
                //        cube.layer = 11;
                //        cube.transform.position = ps.position;
                //        cube.transform.rotation = Quaternion.Euler(ps.rotation.x, ps.rotation.y, ps.rotation.z);
                //        cube.transform.localScale = ps.scale;

                //        //TO-DO: Use real numbers for margins - now 20m

                //        int parkingMargin = 20;
                //        List<Vector3> spots = new List<Vector3>();
                //        for (int i = (int)(-(ps.scale.x / 2)) + parkingMargin; i <= (int)(ps.scale.x / 2) - parkingMargin; i += parkingMargin)
                //        {
                //            for (int j = (int)(-(ps.scale.z / 2)) + parkingMargin; j <= (int)(ps.scale.z / 2) - parkingMargin; j += parkingMargin)
                //            {
                //                Vector3 v = new Vector3((float)i, 0.0f, (float)j);
                //                spots.Add(v);
                //            }
                //        }
                //        ps.parkingSpots = spots;
                //        ps.remainingSpots = ps.parkingSpots.Count;

                //        Parking newStructure = cube.AddComponent<Parking>();
                //        ps.parked = new Dictionary<Vector3, GameObject>();
                //        ps.vehicleAt = new Dictionary<GameObject, Vector3>();
                //        ps.reserved = new Dictionary<GameObject, Vector3>();
                //        newStructure.parkingInfo = ps;


                //        _parkingStructures.Add(cube, ps);
                //    }
                //}
            }

           // path = "RestrictionZones/";
            foreach (var kvp in city.RestrictionZones)
            {
                var rZ = kvp.Value;
                InstantiateRestrictionZone(kvp.Key, rZ);

                //if (rZ.Type.Contains("generic"))
                //{
                //    GameObject newZone = null;
                //    if (rZ.Type.Contains("rectangular"))
                //    {
                //        newZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //        newZone.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height, rZ.Scale.z);
                //    }
                //    else
                //    {
                //        newZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                //        newZone.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height / 2, rZ.Scale.z);
                //    }
                //    newZone.transform.position = new Vector3(rZ.Position.x, rZ.Height / 2, rZ.Position.z);
                //    newZone.transform.rotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
                //    newZone.name = "RestrictionZone_" + rZ.Type;
                //    newZone.tag = "RestrictionZone";
                //    newZone.layer = 8;
                //    newZone.AddComponent<MeshRenderer>();
                //    newZone.GetComponent<MeshRenderer>().material = _restrictionZoneMaterial;
                //    _restrictionZones.Add(newZone);
                //}
                //else if (rZ.Type.Contains("Class"))
                //{
                //    rZ.StepElevs.Add(rZ.Height);
                //    for (int i = 0; i < rZ.StepElevs.Count - 1; i++)
                //    {
                //        float this_cylinder_center_y = (rZ.StepElevs[i] + rZ.StepElevs[i + 1]) / 2.0f;
                //        float this_cylinder_height_half = (rZ.StepElevs[i + 1] - rZ.StepElevs[i]) / 2.0f;
                //        float this_cylinder_radius = rZ.Radii[i];
                //        GameObject newZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                //        newZone.transform.position = new Vector3(rZ.Position.x, this_cylinder_center_y, rZ.Position.z);
                //        newZone.transform.localScale = new Vector3(this_cylinder_radius, this_cylinder_height_half, this_cylinder_radius);
                //        newZone.name = "RestrictionZone_" + rZ.Type + "_" + i.ToString();
                //        newZone.tag = "RestrictionZone";
                //        newZone.layer = 8;
                //        //newwZone.AddComponent<MeshRenderer>();
                //        newZone.GetComponent<MeshRenderer>().material = _restrictionZoneMaterial;
                //        _restrictionZones.Add(newZone);
                //    }
                //}
            }
        }
    }
}
