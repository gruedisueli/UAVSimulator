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
using Assets.Scripts.UI.Events;

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

        private UpdateTool[] _modifyPanels;
        private AddTool[] _addElements;
        private RemoveTool[] _removeElements;

        private SceneElementBase _selectedElement = null;
        private SceneElementBase _workingCopy = null;

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

            //gather UI elments
            _modifyPanels = (UpdateTool[])FindObjectsOfType(typeof(UpdateTool));
            _addElements = (AddTool[])FindObjectsOfType(typeof(AddTool));
            _removeElements = (RemoveTool[])FindObjectsOfType(typeof(RemoveTool));

            //event subscription
            foreach(var m in _modifyPanels)
            {
                m.ElementUpdatedEvent += ElementUpdate;
                m.CommitChangeEvent += CommitUpdates;
                m.StartUpdatingEvent += StartModifying;
            }
            foreach(var a in _addElements)
            {
                a.ElementAddedEvent += AddElement;
            }
            foreach(var r in _removeElements)
            {
                r.ElementRemovedEvent += RemoveElement;
            }
        }

        private void OnDestroy()
        {
            //event unsubscription
            foreach (var m in _modifyPanels)
            {
                m.ElementUpdatedEvent -= ElementUpdate;
                m.CommitChangeEvent -= CommitUpdates;
                m.StartUpdatingEvent -= StartModifying;
            }
            foreach (var a in _addElements)
            {
                a.ElementAddedEvent -= AddElement;
            }
            foreach (var r in _removeElements)
            {
                r.ElementRemovedEvent -= RemoveElement;
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

        /// <summary>
        /// Called when selecting scene element
        /// </summary>
        private void SelectElement(SceneElementBase sE)
        {
            _selectedElement = sE;
        }

        /// <summary>
        /// Called when deselecting a scene element.
        /// </summary>
        private void DeselectElement()
        {
            _selectedElement = null;
        }

        /// <summary>
        /// Called when we start modifying a scene element.
        /// </summary>
        private void StartModifying()
        {
            if (_selectedElement == null)
            {
                Debug.LogError("No object selected");
                return;
            }
            //copy the selected game object
            string guid = Guid.NewGuid().ToString();
            if (_selectedElement is SceneDronePort)
            {
                var sDP = _selectedElement as SceneDronePort;
                var specs = sDP.DronePortSpecs.GetCopy();
                _workingCopy = InstantiateDronePort(guid, specs, false);
            }
            else if (_selectedElement is SceneParkingStructure)
            {
                var sPS = _selectedElement as SceneParkingStructure;
                var specs = sPS.ParkingStructureSpecs.GetCopy();
                _workingCopy = InstantiateParkingStructure(guid, specs, false);
            }
            else if (_selectedElement is SceneRestrictionZone)
            {
                var sRS = _selectedElement as SceneRestrictionZone;
                var specs = sRS.RestrictionZoneSpecs.GetCopy();
                _workingCopy = InstantiateRestrictionZone(guid, specs, false);
            }

        }

        /// <summary>
        /// Called when we finish modification of an element.
        /// </summary>
        private void CommitUpdates(bool commit)
        {
            if (commit) ///throw out old version of selected element and replace in both game and environment
            {
                string guidOld = _selectedElement.Guid;
                string guidNew = Guid.NewGuid().ToString();
                if (_workingCopy is SceneDronePort)
                {
                    var wC = _workingCopy as SceneDronePort;
                    RemoveDronePort(guidOld);
                    EnvironManager.Instance.AddDronePort(guidNew, wC.DronePortSpecs);
                    InstantiateDronePort(guidNew, wC.DronePortSpecs, true);
                }
                else if (_workingCopy is SceneParkingStructure)
                {
                    var wC = _workingCopy as SceneParkingStructure;
                    RemoveParkingStruct(guidOld);
                    EnvironManager.Instance.AddParkingStructure(guidNew, wC.ParkingStructureSpecs);
                    InstantiateParkingStructure(guidNew, wC.ParkingStructureSpecs, true);
                }
                else if (_workingCopy is SceneRestrictionZone)
                {
                    var wC = _workingCopy as SceneRestrictionZone;
                    RemoveRestrictionZone(guidOld);
                    EnvironManager.Instance.AddRestrictionZone(guidNew, wC.RestrictionZoneSpecs);
                    InstantiateRestrictionZone(guidNew, wC.RestrictionZoneSpecs, true);
                }

            }

            //regardless, we want to delete this working copy
            _workingCopy.gameObject.Destroy();
            _workingCopy = null;

            //also set selected to null
            _selectedElement = null;
        }

        /// <summary>
        /// Main method to call when making modifications to objects in scene.
        /// </summary>
        private void ElementUpdate(IUpdateElementArgs args)
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

        /// <summary>
        /// Modifies the working drone port port
        /// </summary>
        private void DronePortUpdate(IUpdateElementArgs args)
        {
            DronePortBase dP = null;
            try
            {
                dP = ((SceneDronePort)_workingCopy).DronePortSpecs;
            }
            catch
            {
                Debug.LogError("Error casting working copy of scene drone port");
                return;
            }
            try
            {
                switch (args.Update.Type)
                {
                    //case UpdatePropertyType.Type:
                    //    {
                    //        //remove old port and reinstantiate
                    //        _dronePorts[args.Guid].gameObject.Destroy();
                    //        _dronePorts.Remove(args.Guid);
                    //        AddNewDronePort(new AddDronePortArgs())
                    //        //envDP.Type = (args.Update as UpdateStringPropertyArg)?.Value;
                    //        //special case: need to reinstantiate
                    //    }
                    case UpdatePropertyType.Position:
                        {
                            dP.Position = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Rotation:
                        {
                            dP.Rotation = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Scale:
                        {
                            dP.Scale = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.StandByPos:
                        {
                            dP.StandbyPosition = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.LandingQueueHead:
                        {
                            dP.LandingQueueHead = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.LandingQueueDirection:
                        {
                            dP.LandingQueueDirection = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.LandingPoint:
                        {
                            dP.LandingPoint = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.MaxVehicleSize:
                        {
                            dP.MaximumVehicleSize = (args.Update as UpdateFloatPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.IsMountable:
                        {
                            dP.IsMountable = (args.Update as UpdateBoolPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.IsOnTheGround:
                        {
                            dP.IsOnTheGround = (args.Update as UpdateBoolPropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.IsScalable:
                        {
                            dP.IsScalable = (args.Update as UpdateBoolPropertyArg).Value;
                            break;
                        }
                }
            }
            catch
            {
                Debug.LogError("Casting error in drone port property update");
                return;
            }

            _workingCopy.UpdateGameObject();
        }

        /// <summary>
        /// Modifies a parking structure
        /// </summary>
        private void ParkingStructureUpdate(IUpdateElementArgs args)
        {
            ParkingStructureBase pS = null;
            try
            {
                pS = ((SceneParkingStructure)_workingCopy).ParkingStructureSpecs;
            }
            catch
            {
                Debug.LogError("Error casting working copy of scene parking structure");
                return;
            }
            try
            {
                switch (args.Update.Type)
                {
                    //case UpdatePropertyType.Type:
                    //    {
                    //        //remove old port and reinstantiate
                    //        _dronePorts[args.Guid].gameObject.Destroy();
                    //        _dronePorts.Remove(args.Guid);
                    //        AddNewDronePort(new AddDronePortArgs())
                    //        //envDP.Type = (args.Update as UpdateStringPropertyArg)?.Value;
                    //        //special case: need to reinstantiate
                    //    }
                    case UpdatePropertyType.Position:
                        {
                            pS.Position = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Rotation:
                        {
                            pS.Rotation = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.Scale:
                        {
                            pS.Scale = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.StandByPos:
                        {
                            pS.StandbyPosition = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.LandingQueueHead:
                        {
                            pS.LandingQueueHead = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                    case UpdatePropertyType.LandingQueueDirection:
                        {
                            pS.LandingQueueDirection = (args.Update as UpdateVector3PropertyArg).Value;
                            break;
                        }
                }
            }
            catch
            {
                Debug.LogError("Casting error in parking strcuture property update");
                return;
            }

            _workingCopy.UpdateGameObject();
        }

        /// <summary>
        /// Modifies a restriction zone
        /// </summary>
        private void RestrictionZoneUpdate(IUpdateElementArgs args)
        {
            RestrictionZoneBase rZ = null;
            try
            {
                rZ = ((SceneRestrictionZone)_workingCopy).RestrictionZoneSpecs;
            }
            catch
            {
                Debug.LogError("Error casting working copy of scene restriction zone");
                return;
            }

            rZ.UpdateParams(args.Update);

            _workingCopy.UpdateGameObject();
        }

        /// <summary>
        /// Adds any type of element to scene.
        /// </summary>
        private void AddElement(IAddElementArgs args)
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

        /// <summary>
        /// Removes any type of element from scene.
        /// </summary>
        private void RemoveElement(IRemoveElementArgs args)
        {
            switch (args.Family)
            {
                case ElementFamily.DronePort:
                    {
                        RemoveDronePort(args.Guid);
                        break;
                    }
                case ElementFamily.ParkingStruct:
                    {
                        RemoveParkingStruct(args.Guid);
                        break;
                    }
                case ElementFamily.RestrictionZone:
                    {
                        RemoveRestrictZone(args.Guid);
                        break;
                    }
            }
        }

        /// <summary>
        /// Adds a new drone port from UI.
        /// </summary>
        private void AddNewDronePort(AddDronePortArgs args)
        {
            DronePortBase dP = null;
            switch (args.Category)
            {
                case DronePortCategory.Rect:
                    {
                        dP = new DronePortRect(args.Position);
                        break;
                    }
                case DronePortCategory.Custom:
                    {
                        if (EnvironManager.Instance.DronePortAssets.ContainsKey(args.Type))
                        {
                            dP = EnvironManager.Instance.DronePortAssets[args.Type].Specs;
                            dP.Position = args.Position;
                            break;
                        }
                        else
                        {
                            Debug.LogError("Added drone port type not found in assets");
                            dP = null;
                            break;
                        }
                    }
            }
            if (dP == null)
            {
                return;
            }
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddDronePort(guid, dP);
            InstantiateDronePort(guid, dP, true);
        }

        /// <summary>
        /// Adds a new parking structure from UI.
        /// </summary>
        private void AddNewParkingStruct(AddParkingStructArgs args)
        {
            ParkingStructureBase pS = null;
            switch (args.Category)
            {
                case ParkingStructCategory.Rect:
                    {
                        pS = new ParkingStructureRect(args.Position);
                        break;
                    }
                case ParkingStructCategory.Custom:
                    {
                        if (EnvironManager.Instance.ParkingStructAssets.ContainsKey(args.Type))
                        {
                            pS = EnvironManager.Instance.ParkingStructAssets[args.Type].Specs;
                            pS.Position = args.Position;
                            break;
                        }
                        else
                        {
                            Debug.LogError("Added parking structure type not found in assets");
                            pS = null;
                            break;
                        }
                    }
            }
            if (pS == null)
            {
                return;
            }
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddParkingStructure(guid, pS);
            InstantiateParkingStructure(guid, pS, true);
        }

        /// <summary>
        /// Adds a new restriction zone from UI.
        /// </summary>
        private void AddNewRestrictZone(AddRestrictionZoneArgs args)
        {
            RestrictionZoneBase rZ = null;
            switch (args.Category)
            {
                case RestrictionZoneCategory.Rect:
                    {
                        rZ = new RestrictionZoneRect(args.Position);
                        break;
                    }
                case RestrictionZoneCategory.Cylindrical:
                    {
                        rZ = new RestrictionZoneCyl(args.Position);
                        break;
                    }
                case RestrictionZoneCategory.CylindricalStacked:
                    {
                        rZ = new RestrictionZoneCylStack(args.Position);
                        break;
                    }
            }
            if (rZ == null)
            {
                return;
            }
            string guid = Guid.NewGuid().ToString();
            EnvironManager.Instance.AddRestrictionZone(guid, rZ);
            InstantiateRestrictionZone(guid, rZ, true);
        }

        /// <summary>
        /// Removes drone port.
        /// </summary>
        private void RemoveDronePort(string guid)
        {
            EnvironManager.Instance.RemoveDronePort(guid);
            if (_dronePorts.ContainsKey(guid))
            {
                _dronePorts[guid].gameObject.Destroy();
                _dronePorts.Remove(guid);
            }
            else
            {
                Debug.LogError("Drone port selected for removal not found in dictionary");
            }
        }

        /// <summary>
        /// Removes parking structure.
        /// </summary>
        private void RemoveParkingStruct(string guid)
        {
            EnvironManager.Instance.RemoveParkingStructure(guid);
            if (_parkingStructures.ContainsKey(guid))
            {
                _parkingStructures[guid].gameObject.Destroy();
                _parkingStructures.Remove(guid);
            }
            else
            {
                Debug.LogError("Parking structure selected for removal not found in dictionary");
            }
        }

        /// <summary>
        /// Removes restriction zone.
        /// </summary>
        private void RemoveRestrictZone(string guid)
        {
            EnvironManager.Instance.RemoveRestrictionZone(guid);
            if (_restrictionZones.ContainsKey(guid))
            {
                _restrictionZones[guid].gameObject.Destroy();
                _restrictionZones.Remove(guid);
            }
            else
            {
                Debug.LogError("Restrction zone selected for removal not found in dictionary");
            }
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
        private SceneDronePort InstantiateDronePort(string guid, DronePortBase dP, bool register)
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
            }

            return sDP;
        }

        /// <summary>
        /// Instantiates a parking structure. Does not update enviornment.
        /// </summary>
        private SceneParkingStructure InstantiateParkingStructure(string guid, ParkingStructureBase pS, bool register)
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
            }

            return sPS;
        }

        /// <summary>
        /// Instantiates a restriction zone. Does not update enviornment.
        /// </summary>
        private SceneRestrictionZone InstantiateRestrictionZone(string guid, RestrictionZoneBase rZ, bool register)
        {
            var zoneParent = new GameObject();
            var sRZ = zoneParent.AddComponent<SceneRestrictionZone>();
            sRZ.Initialize(guid, rZ, _restrictionZoneMaterial);

            if (register)
            {
                _restrictionZones.Add(guid, sRZ);
            }

            return sRZ;
        }

        /// <summary>
        /// Instantiates a generic rectangular drone port in project. Does not update environment.
        /// </summary>
        private SceneDronePort InstantiateRectDronePort(string guid, DronePortRect dP, bool register)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sDP = clone.AddComponent<SceneDronePort>();
            sDP.Initialize(dP, guid);

            if (register)
            {
                _dronePorts.Add(guid, sDP);
            }

            return sDP;
        }

        /// <summary>
        /// Instantiates a custom drone port in the project. Does not update environment.
        /// </summary>
        private SceneDronePort InstantiateCustomDronePort(string guid, GameObject prefab, DronePortCustom dP, bool register)
        {
            var clone = Instantiate(prefab/*, dP.Position, Quaternion.Euler(dP.Rotation.x, dP.Rotation.y, dP.Rotation.z)*/);
            var sDP = clone.AddComponent<SceneDronePort>();
            sDP.Initialize(dP, guid);

            if (register)
            {
                _dronePorts.Add(guid, sDP);
            }

            return sDP;
        }

        /// <summary>
        /// Instantiates a custom parking structure in the project. Does not update environment.
        /// </summary>
        private SceneParkingStructure InstantiateCustomParkingStruct(string guid, GameObject prefab, ParkingStructureCustom pS, bool register)
        {
            var clone = Instantiate(prefab/*, pS.Position, Quaternion.Euler(pS.Rotation.x, pS.Rotation.y, pS.Rotation.z)*/);
            var sPS = clone.AddComponent<SceneParkingStructure>();
            sPS.Initialize(pS, guid);

            if (register)
            {
                _parkingStructures.Add(guid, sPS);
            }

            return sPS;
        }


        /// <summary>
        /// Instantiates a rectangular parking structure in the project. Does not update environment.
        /// </summary>
        private SceneParkingStructure InstantiateRectParkingStruct(string guid, ParkingStructureRect pS, bool register)
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sPS = clone.AddComponent<SceneParkingStructure>();
            sPS.Initialize(pS, guid);

            if (register)
            {
                _parkingStructures.Add(guid, sPS);
            }

            return sPS;
        }

        public void InstantiateObjects()
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
    }
}
