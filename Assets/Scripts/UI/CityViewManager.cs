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
using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Manages the city view. Only functions specific to city view should be here. If it can be generalized, put it in the base class.
    /// </summary>
    public class CityViewManager : SceneManagerBase
    {
        private FOA_RangeAroundTransformTileProvider _tileProvider;
        private GameObject _cityCenter;

        protected override void Init()
        {
            _cityCenter = new GameObject();
            _cityCenter.name = "City Center";
            
            //get tile provider
            _tileProvider = FindObjectOfType<FOA_RangeAroundTransformTileProvider>(true);
            if (_tileProvider == null)
            {
                Debug.LogError("Tile provider not found");
                return;
            }

            var eC = EnvironManager.Instance;
            //var city = eC.Environ.GetCity(eC.ActiveCity);
            var city = eC.GetCurrentCity();
            if (city != null)
            {
                _cityCenter.transform.position = city.CityStats.WorldPos;
                var options = new FOA_RangeAroundTransformTileProviderOptions();
                options._eastExt = city.CityStats.EastExt;
                options._westExt = city.CityStats.WestExt;
                options._northExt = city.CityStats.NorthExt;
                options._southExt = city.CityStats.SouthExt;
                options._targetTransform = _cityCenter.transform;
                _tileProvider.SetOptions(options);
                _mainCamera.transform.position = new Vector3(city.CityStats.WorldPos.x, 1000, city.CityStats.WorldPos.z);
                _abstractMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.CITY_ZOOM_LEVEL);
            }
        }

        protected override void OnDestroyDerived()
        {

        }

        #region MODIFICATION

        /// <summary>
        /// Main method to call when making modifications to objects in scene.
        /// </summary>
        protected override void ElementModify(IModifyElementArgs args)
        {
            if (_workingCopy == null)
            {
                Debug.LogError("Working copy is null");
                return;
            }
            if (_workingCopy is SceneDronePort)
            {
                DronePortUpdate(args);
            }
            else if (_workingCopy is SceneParkingStructure)
            {
                ParkingStructureUpdate(args);
            }
            else if (_workingCopy is SceneRestrictionZone)
            {
                RestrictionZoneUpdate(args);
            }
        }

        #endregion

        #region ADD/REMOVE ELEMENTS

        /// <summary>
        /// Adds any type of element to scene.
        /// </summary>
        protected override void AddElement(IAddElementArgs args)
        {
            if (args is AddDronePortArgs)
            {
                AddNewDronePort(args as AddDronePortArgs);
            }
            else if (args is AddParkingStructArgs)
            {
                AddNewParkingStruct(args as AddParkingStructArgs);
            }
            else if (args is AddRestrictionZoneArgs)
            {
                AddNewRestrictZone(args as AddRestrictionZoneArgs);
            }
            else
            {
                Debug.LogError("Added elements arguments of unrecognized type");
            }
        }

        #endregion

        #region INSTANTIATION

        protected override SceneDronePort InstantiateDronePort(string guid, DronePortBase dP, bool register)
        {
            SceneDronePort sDP = null;
            if (dP is DronePortRect)
            {
                sDP = InstantiateRectDronePort(guid, dP as DronePortRect, register);
            }
            else if (dP is DronePortCustom)
            {
                var pfb = EnvironManager.Instance.DronePortAssets[dP.Type].Prefab;
                sDP = InstantiateCustomDronePort(guid, pfb, dP as DronePortCustom, register);
            }
            else
            {
                Debug.LogError("Drone port type requested for instantiation not recognized");
                return null;
            }

            sDP.OnSceneElementSelected += SelectElement;

            return sDP;
        }

        protected override SceneParkingStructure InstantiateParkingStructure(string guid, ParkingStructureBase pS, bool register)
        {
            SceneParkingStructure sPS = null;
            if (pS is ParkingStructureRect)
            {
                sPS = InstantiateRectParkingStruct(guid, pS as ParkingStructureRect, register);
            }
            else if (pS is ParkingStructureCustom)
            {
                var pfb = EnvironManager.Instance.ParkingStructAssets[pS.Type].Prefab;
                sPS = InstantiateCustomParkingStruct(guid, pfb, pS as ParkingStructureCustom, register);
            }
            else
            {
                Debug.LogError("Parking structure type requested for instantiation not recognized");
                return null;
            }

            sPS.OnSceneElementSelected += SelectElement;

            return sPS;
        }

        protected override SceneRestrictionZone InstantiateRestrictionZone(string guid, RestrictionZoneBase rZ, bool register)
        {
            var zoneParent = new GameObject();
            var sRZ = zoneParent.AddComponent<SceneRestrictionZone>();
            sRZ.Initialize(guid, rZ, _restrictionZoneMaterial);

            if (register)
            {
                RestrictionZones.Add(guid, sRZ);
            }

            sRZ.OnSceneElementSelected += SelectElement;

            return sRZ;
        }

        protected override SceneDronePort InstantiateRectDronePort(string guid, DronePortRect dP, bool register)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sDP = clone.AddComponent<SceneDronePort>();
            sDP.Initialize(dP, guid);

            if (register)
            {
                DronePorts.Add(guid, sDP);
            }

            sDP.OnSceneElementSelected += SelectElement;

            return sDP;
        }

        protected override SceneDronePort InstantiateCustomDronePort(string guid, GameObject prefab, DronePortCustom dP, bool register)
        {
            var clone = Instantiate(prefab/*, dP.Position, Quaternion.Euler(dP.Rotation.x, dP.Rotation.y, dP.Rotation.z)*/);
            var sDP = clone.AddComponent<SceneDronePort>();
            clone.AddComponent<BoxCollider>();
            sDP.Initialize(dP, guid);

            if (register)
            {
                DronePorts.Add(guid, sDP);
            }

            sDP.OnSceneElementSelected += SelectElement;

            return sDP;
        }

        protected override SceneParkingStructure InstantiateCustomParkingStruct(string guid, GameObject prefab, ParkingStructureCustom pS, bool register)
        {
            var clone = Instantiate(prefab/*, pS.Position, Quaternion.Euler(pS.Rotation.x, pS.Rotation.y, pS.Rotation.z)*/);
            var sPS = clone.AddComponent<SceneParkingStructure>();
            clone.AddComponent<BoxCollider>();
            sPS.Initialize(pS, guid);

            if (register)
            {
                ParkingStructures.Add(guid, sPS);
            }

            sPS.OnSceneElementSelected += SelectElement;

            return sPS;
        }

        protected override SceneParkingStructure InstantiateRectParkingStruct(string guid, ParkingStructureRect pS, bool register)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sPS = clone.AddComponent<SceneParkingStructure>();
            sPS.Initialize(pS, guid);

            if (register)
            {
                ParkingStructures.Add(guid, sPS);
            }

            sPS.OnSceneElementSelected += SelectElement;

            return sPS;
        }

        protected override SceneCity InstantiateCity(string guid, CityOptions cityOptions, bool register)
        {
            throw new NotImplementedException();
        }

        protected override void InstantiateObjects()
        {
            // (Eunu? stll relevant?) TO-DO: Also register the restriction zones for drone ports and parking structures

            var eM = EnvironManager.Instance;
            var city = eM.GetCurrentCity();
            foreach (var kvp in city.DronePorts)
            {
                var dP = kvp.Value;
                InstantiateDronePort(kvp.Key, dP, true);
            }

            foreach (var kvp in city.ParkingStructures)
            {
                var pS = kvp.Value;
                InstantiateParkingStructure(kvp.Key, pS, true);
            }

            foreach (var kvp in city.RestrictionZones)
            {
                var rZ = kvp.Value;
                InstantiateRestrictionZone(kvp.Key, rZ, true);
            }
        }

        #endregion
    }
}
