using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Assets.Scripts.Environment;
using Assets.Scripts.Serialization;

using UnityEngine;

using Mapbox.Unity.Map;

using Assets.Scripts.UI.Panels;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI
{
    public abstract class SceneManagerBase : MonoBehaviour
    {
        #region PUBLIC FIELDS

        public Material _restrictionZoneMaterial;

        #endregion

        #region PROTECTED FIELDS

        protected AbstractMap _abstractMap;

        protected Camera _mainCamera;

        protected UpdateTool[] _modifyPanels;
        protected AddTool[] _addElements;
        protected RemoveTool[] _removeElements;
        protected SavePrompt _savePrompt;

        protected SceneElementBase _selectedElement = null;
        protected SceneElementBase _workingCopy = null;

        #endregion

        #region PROPERTIES

        public Dictionary<string, SceneDronePort> DronePorts { get; protected set; }
        public Dictionary<string, SceneParkingStructure> ParkingStructures { get; protected set; }
        public Dictionary<string, SceneRestrictionZone> RestrictionZones { get; protected set; }

        public List<GameObject> DestinationCollections { get; protected set; }
        public Dictionary<GameObject, List<GameObject>> Routes { get; protected set; }

        #endregion

        private void Start()
        {
            DronePorts = new Dictionary<string, SceneDronePort>();
            ParkingStructures = new Dictionary<string, SceneParkingStructure>();
            DestinationCollections = new List<GameObject>();
            Routes = new Dictionary<GameObject, List<GameObject>>();
            RestrictionZones = new Dictionary<string, SceneRestrictionZone>();

            //get mapbox abstract map
            _abstractMap = (AbstractMap)FindObjectOfType(typeof(AbstractMap));

            //get main camera
            _mainCamera = Camera.main;

            //gather UI elments
            _modifyPanels = (UpdateTool[])FindObjectsOfType(typeof(UpdateTool));
            _addElements = (AddTool[])FindObjectsOfType(typeof(AddTool));
            _removeElements = (RemoveTool[])FindObjectsOfType(typeof(RemoveTool));
            _savePrompt = (SavePrompt)FindObjectOfType(typeof(SavePrompt));

            //event subscription
            foreach (var m in _modifyPanels)
            {
                m.ElementUpdatedEvent += ElementUpdate;
                m.CommitChangeEvent += CommitUpdates;
                m.StartUpdatingEvent += StartModifying;
            }
            foreach (var a in _addElements)
            {
                a.ElementAddedEvent += AddElement;
            }
            foreach (var r in _removeElements)
            {
                r.ElementRemovedEvent += RemoveElement;
            }

            Init();

            InstantiateObjects();

            CreateRoutes();
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
        /// All derived classes should call this instead of "Start()", for use in view-specific initialization
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// What objects we want to instantiate will vary from scene to scene.
        /// </summary>
        protected abstract void InstantiateObjects();


        #region CHANGE SCENE/QUIT

        /// <summary>
        /// Initiates process of going to main menu
        /// </summary>
        protected void GoMain()
        {
            _savePrompt.GoMain();
        }

        /// <summary>
        /// Initiates process of quitting
        /// </summary>
        protected void Quit()
        {
            _savePrompt.Quit();
        }

        #endregion

        #region SELECT/DESELECT

        /// <summary>
        /// Called when selecting scene element
        /// </summary>
        protected void SelectElement(SceneElementBase sE)
        {
            if (_selectedElement == null) //only change selection if we don't have something selected
            {
                _selectedElement = sE;
            }
        }

        /// <summary>
        /// Called when deselecting a scene element.
        /// </summary>
        protected void DeselectElement()
        {
            _selectedElement = null;
        }

        #endregion

        #region MODIFY ELEMENTS

        /// <summary>
        /// Called when we start modifying a scene element.
        /// </summary>
        protected void StartModifying()
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
        protected void CommitUpdates(bool commit)
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
                    RemoveParkingStructure(guidOld);
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
        protected void ElementUpdate(IUpdateElementArgs args)
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
        protected void DronePortUpdate(IUpdateElementArgs args)
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
        protected void ParkingStructureUpdate(IUpdateElementArgs args)
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
        protected void RestrictionZoneUpdate(IUpdateElementArgs args)
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
        #endregion

        #region ADD/REMOVE ELEMENTS

        /// <summary>
        /// Adds any type of element to scene.
        /// </summary>
        protected void AddElement(IAddElementArgs args)
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
        protected void RemoveElement(IRemoveElementArgs args)
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
                        RemoveParkingStructure(args.Guid);
                        break;
                    }
                case ElementFamily.RestrictionZone:
                    {
                        RemoveRestrictionZone(args.Guid);
                        break;
                    }
            }
        }

        /// <summary>
        /// Adds a new drone port from UI.
        /// </summary>
        protected void AddNewDronePort(AddDronePortArgs args)
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
        protected void AddNewParkingStruct(AddParkingStructArgs args)
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
        protected void AddNewRestrictZone(AddRestrictionZoneArgs args)
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
        /// Attempts to remove drone port from scene and environment.
        /// </summary>
        protected void RemoveDronePort(string guid)
        {
            EnvironManager.Instance.RemoveDronePort(guid);
            if (DronePorts.ContainsKey(guid))
            {
                DronePorts[guid].OnSceneElementSelected -= SelectElement;
                DronePorts[guid].Destroy();
                DronePorts.Remove(guid);
            }
            else
            {
                Debug.LogError("Drone port for removal not found in scene dictionary");
            }
        }

        /// <summary>
        /// Attempts to remove parking structure from scene and environment.
        /// </summary>
        protected void RemoveParkingStructure(string guid)
        {
            EnvironManager.Instance.RemoveParkingStructure(guid);
            if (ParkingStructures.ContainsKey(guid))
            {
                ParkingStructures[guid].OnSceneElementSelected -= SelectElement;
                ParkingStructures[guid].Destroy();
                ParkingStructures.Remove(guid);
            }
            else
            {
                Debug.LogError("Parking structure for removal not found in scene dictionary");
            }
        }

        /// <summary>
        /// Attempts to remove restriction zone from scene and environment.
        /// </summary>
        protected void RemoveRestrictionZone(string guid)
        {
            EnvironManager.Instance.RemoveRestrictionZone(guid);
            if (RestrictionZones.ContainsKey(guid))
            {
                RestrictionZones[guid].OnSceneElementSelected -= SelectElement;
                RestrictionZones[guid].Destroy();
                RestrictionZones.Remove(guid);
            }
            else
            {
                Debug.LogError("Restriction zone for removal not found in scene dictionary");
            }
        }

        #endregion

        #region INSTANTIATE ELEMENTS

        /// <summary>
        /// Instantiates a rectangular parking structure in the project. Does not update environment.
        /// </summary>
        protected abstract SceneParkingStructure InstantiateRectParkingStruct(string guid, ParkingStructureRect pS, bool register);

        /// <summary>
        /// Instantiates a custom parking structure in the project. Does not update environment.
        /// </summary>
        protected abstract SceneParkingStructure InstantiateCustomParkingStruct(string guid, GameObject prefab, ParkingStructureCustom pS, bool register);

        /// <summary>
        /// Instantiates a custom drone port in the project. Does not update environment.
        /// </summary>
        protected abstract SceneDronePort InstantiateCustomDronePort(string guid, GameObject prefab, DronePortCustom dP, bool register);

        /// <summary>
        /// Instantiates a generic rectangular drone port in project. Does not update environment.
        /// </summary>
        protected abstract SceneDronePort InstantiateRectDronePort(string guid, DronePortRect dP, bool register);

        /// <summary>
        /// Instantiates a restriction zone. Does not update enviornment.
        /// </summary>
        protected abstract SceneRestrictionZone InstantiateRestrictionZone(string guid, RestrictionZoneBase rZ, bool register);

        /// <summary>
        /// Instantiates a parking structure. Does not update enviornment.
        /// </summary>
        protected abstract SceneParkingStructure InstantiateParkingStructure(string guid, ParkingStructureBase pS, bool register);

        /// <summary>
        /// Instantiates a drone port. Does not update enviornment.
        /// </summary>
        protected abstract SceneDronePort InstantiateDronePort(string guid, DronePortBase dP, bool register);

        #endregion

        #region SIMULATION FUNCTIONS

        public void CreateRoutes()
        {

            DestinationCollections = new List<GameObject>();
            foreach (var kvp in DronePorts)
            {
                DestinationCollections.Add(kvp.Value.gameObject);
            }


            // create straight paths first (elevation == 0 means straight paths)
            // paths whose elevation is closest to the assigned elevation will be examined first
            // if there is an obstacle in the middle, construct a walkaround path and register with the new elevation

            for (int i = 0; i < DestinationCollections.Count; i++)
            {
                for (int j = 0; j < DestinationCollections.Count; j++)
                {
                    if (i != j)
                    {
                        GameObject origin = DestinationCollections[i];
                        GameObject destination = DestinationCollections[j];
                        if (Vector3.Distance(origin.transform.position, destination.transform.position) < EnvironSettings.RANGE_LIMIT)
                        {
                            if (!Routes.ContainsKey(origin)) Routes.Add(origin, new List<GameObject>());

                            List<GameObject> this_origin_adjacent_nodes = Routes[origin];
                            this_origin_adjacent_nodes.Add(destination);
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Gets number of parking spots in current scene.
        /// </summary>
        public int GetParkingCapacity()
        {
            int parking_capacity = 0;
            foreach (var kvp in ParkingStructures)
            {
                parking_capacity += kvp.Value.ParkingStructureSpecs.ParkingSpots.Count;
            }
            return parking_capacity;
        }

        #endregion

        
    }
}
