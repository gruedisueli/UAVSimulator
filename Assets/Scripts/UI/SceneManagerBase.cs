using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Assets.Scripts.Environment;
using Assets.Scripts.Serialization;

using UnityEngine;
using UnityEngine.SceneManagement;

using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;

using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.DataStructure;

using DelaunatorSharp;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Base class for managers of a scene in the game. Contains functions that should not vary from sceme to scene, whether or not they are needed in each scene.
    /// Anything that can be generalized should go here.
    /// Acts as a listener for many types of events in the scene.
    /// Receives and sends messages to UI.
    /// </summary>
    public abstract class SceneManagerBase : MonoBehaviour
    {
        #region PROTECTED FIELDS

        public AbstractMap _abstractMap;
        public AbstractMap _largeScaleMap;
        public Canvas _canvas;

        protected Camera _mainCamera;

        protected VehicleControlSystem _vehicleControlSystem;

        protected ModifyTool[] _modifyTools;//certain modify tools that are not for modifying scene elements but simulation, etc. Modify tools for scene elements get instantiated dynamically.
        protected AddTool[] _addTools;
        protected SceneChangeTool[] _sceneChangeTools;
        protected DeselectTool[] _deselectTools;
        protected SavePrompt _savePrompt;
        protected PlayPauseTool _playPause;
        protected DownloadAirspaceTool _downloadAirspaceTool;

        protected SceneElementBase _selectedElement = null;
        protected SceneElementBase _workingCopy = null;
        protected ElementInfoPanel _currentInfoPanel = null;

        protected Canvas _mainCanvas;

        protected float _airspaceYScale = 10;

        #endregion

        #region PROPERTIES

        public Dictionary<string, SceneCity> Cities { get; protected set; } = new Dictionary<string, SceneCity>();
        public Dictionary<string, SceneDronePort> DronePorts { get; protected set; }
        public Dictionary<string, SceneParkingStructure> ParkingStructures { get; protected set; }
        public Dictionary<string, SceneRestrictionZone> RestrictionZones { get; protected set; }

        public List<GameObject> DestinationCollections { get; protected set; }
        public Assets.Scripts.DataStructure.Network network { get; protected set; }


        #endregion

        private void Start()
        {
            DronePorts = new Dictionary<string, SceneDronePort>();
            ParkingStructures = new Dictionary<string, SceneParkingStructure>();
            DestinationCollections = new List<GameObject>();
            RestrictionZones = new Dictionary<string, SceneRestrictionZone>();
            

        //get main canvase
        _mainCanvas = GetComponentInChildren<Canvas>(true);
            if (_mainCanvas == null)
            {
                Debug.LogError("Main canvas not found in children of scene manager");
                return;
            }

            //get mapbox abstract map
            //_abstractMap = FindObjectOfType<AbstractMap>(true);
            if (_abstractMap == null)
            {
                Debug.LogError("Abstract map not specified");
                return;
            }
            if (_largeScaleMap == null) //it's ok not to specify this.
            {
                Debug.Log("Large scale map not specified for this view.");
            }

            //get main camera
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("Main camera not found");
                return;
            }

            //get vehicle control system
            _vehicleControlSystem = FindObjectOfType<VehicleControlSystem>(true);
            if (_vehicleControlSystem == null)
            {
                Debug.LogError("Vehicle control system not found");
            }

            //gather UI elments
            _modifyTools = FindObjectsOfType<ModifyTool>(true);
            _addTools = FindObjectsOfType<AddTool>(true);
            _sceneChangeTools = FindObjectsOfType<SceneChangeTool>(true);
            _deselectTools = FindObjectsOfType<DeselectTool>(true);
            _savePrompt = FindObjectOfType<SavePrompt>(true);
            if (_savePrompt == null)
            {
                Debug.LogError("Save prompt not found");
                return;
            }
            _playPause = FindObjectOfType<PlayPauseTool>(true);
            if (_playPause == null)
            {
                Debug.LogError("Play pause tool not found");
                return;
            }
            _downloadAirspaceTool = FindObjectOfType<DownloadAirspaceTool>(true);

            //event subscription
            //QUESTION: Why register our tools directly on the scene manager and not sub-panels?
            //ANSWER: Because by registering here, we are not wedded to a specific tool hierarchy and can rearrange our tools regardless of sub-panels, etc.
            foreach (var a in _addTools)
            {
                a.ElementAddedEvent += AddElement;
            }
            foreach (var t in _modifyTools)
            {
                t.OnElementModified += ElementModify;
            }
            foreach (var t in _sceneChangeTools)
            {
                t.OnSceneChange += ChangeScene;
            }
            foreach(var t in _deselectTools)
            {
                t.OnDeselect += DeselectElement;
            }
            _playPause.OnPlayPause += PlayPause;
            if (_downloadAirspaceTool != null)
            {
                _downloadAirspaceTool.OnDownloadAirspace += GetAirspaceData;
            }

            Init();

            InstantiateObjects();

            GenerateNetwork();
        }

        private void OnDestroy()
        {
            //event unsubscription
            foreach (var a in _addTools)
            {
                a.ElementAddedEvent -= AddElement;
            }
            foreach (var t in _modifyTools)
            {
                t.OnElementModified -= ElementModify;
            }
            foreach (var t in _sceneChangeTools)
            {
                t.OnSceneChange -= ChangeScene;
            }
            foreach (var t in _deselectTools)
            {
                t.OnDeselect -= DeselectElement;
            }
            foreach (var d in DronePorts)
            {
                d.Value.OnSceneElementSelected -= SelectElement;
            }
            foreach(var p in ParkingStructures)
            {
                p.Value.OnSceneElementSelected -= SelectElement;
            }
            foreach(var r in RestrictionZones)
            {
                r.Value.OnSceneElementSelected -= SelectElement;
            }
            foreach (var c in Cities.Values)
            {
                c.OnSceneElementSelected -= SelectElement;
            }
            _playPause.OnPlayPause -= PlayPause;
            if (_currentInfoPanel != null)
            {
                //unsubscribe from events.
                //start modify
                _currentInfoPanel.StartModifyTool.OnStartModify -= StartModifying;
                //cancel modify
                _currentInfoPanel.ModifyPanel._closeTool.OnClose -= CancelCommit;
                //commit modify
                _currentInfoPanel.ModifyPanel.CommitTool.OnCommit -= CommitUpdates;
                //modification events from tools
                foreach (var t in _currentInfoPanel.ModifyTools)
                {
                    t.OnElementModified -= ElementModify;
                }
                //removal tool
                _currentInfoPanel.RemoveTool.OnSelectedElementRemoved -= RemoveSelectedElement;

                //deselect tool
                _currentInfoPanel.DeselectTool.OnDeselect -= DeselectElement;
                //scene change
                if (_currentInfoPanel is CityInfoPanel)
                {
                    var cP = _currentInfoPanel as CityInfoPanel;
                    cP.GoToCity.OnSceneChange -= ChangeScene;
                }
            }
            if (_downloadAirspaceTool != null)
            {
                _downloadAirspaceTool.OnDownloadAirspace += GetAirspaceData;
            }

            OnDestroyDerived();
        }

        /// <summary>
        /// Called by classes that inherit from this base class
        /// </summary>
        protected abstract void OnDestroyDerived();

        /// <summary>
        /// All derived classes should call this instead of "Start()", for use in view-specific initialization
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// What objects we want to instantiate will vary from scene to scene.
        /// </summary>
        protected abstract void InstantiateObjects();

        #region SIMULATION CONTROL

        /// <summary>
        /// Called whenever we push play/pause button.
        /// </summary>
        protected void PlayPause(object sender, PlayPauseArgs args)
        {
            if (_vehicleControlSystem != null)
            {
                _vehicleControlSystem.PlayPause();
            }
        }

        #endregion

        #region CHANGE SCENE/QUIT

        /// <summary>
        /// Handles ansynchronous loading of scenes.
        /// </summary>
        protected IEnumerator LoadAsyncOperation(int scenePath)
        {
            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("LOADING");
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(scenePath);
            while (loadScene.progress < 1)
            {
                progressBar.SetCompletion(loadScene.progress);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Called when changing scenes in any way, or quitting
        /// </summary>
        protected void ChangeScene(object sender, SceneChangeArgs args)
        {
            switch (args.SceneType)
            {
                case SceneType.City:
                    {
                        GoToCity();
                        break;
                    }
                case SceneType.FindLoc:
                    {
                        GoToFindLoc();
                        break;
                    }
                case SceneType.Main:
                    {
                        GoMain();
                        break;
                    }
                case SceneType.Quit:
                    {
                        Quit();
                        break;
                    }
                case SceneType.Region:
                    {
                        GoToRegion();
                        break;
                    }
                case SceneType.Unset:
                    {
                        Debug.LogError("Scene type not selected");
                        break;
                    }
            }
        }

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

        protected void GoToCity()
        {
            if (_selectedElement is SceneCity)
            {
                EnvironManager.Instance.SetActiveCity(_selectedElement.Guid);
                StartCoroutine(LoadAsyncOperation(UISettings.CITYVIEW_SCENEPATH));
            }
        }

        protected void GoToFindLoc()
        {
            StartCoroutine(LoadAsyncOperation(UISettings.FINDLOCATION_SCENEPATH));
        }


        /// <summary>
        /// Loads up the region view of current environment
        /// </summary>
        protected void GoToRegion()
        {
            StartCoroutine(LoadAsyncOperation(UISettings.REGIONVIEW_SCENEPATH));
        }

        #endregion

        #region SELECT/DESELECT

        /// <summary>
        /// Called when selecting scene element. False if there is already a selected element.
        /// </summary>
        protected virtual bool SelectElement(SceneElementBase sE)
        {
            if (_selectedElement == null) //only change selection if we don't have something selected
            {
                if (sE.gameObject.layer != 13) _selectedElement = sE;
                else _selectedElement = sE.gameObject.transform.parent.gameObject.GetComponent<SceneElementBase>();
                _selectedElement.SetSelectedState(true);

                //instantiate and turn on info panel
                GameObject clone = null;
                if (_selectedElement is SceneCity)
                {
                    clone = Instantiate(EnvironManager.Instance.CityInfoPanelPrefab);
                }
                else if (_selectedElement is SceneDronePort)
                {
                    clone = Instantiate(EnvironManager.Instance.DronePortInfoPanelPrefab);
                }
                else if (_selectedElement is SceneParkingStructure)
                {
                    clone = Instantiate(EnvironManager.Instance.ParkingInfoPanelPrefab);
                }
                else if (_selectedElement is SceneRestrictionZone)
                {
                    clone = Instantiate(EnvironManager.Instance.RestrictionInfoPanelPrefab);
                }
                bool success = InitInfoPanel(clone);
                return success;
            }

            return false;
        }

        /// <summary>
        /// Final setup of info panel. True on success, false if not.
        /// </summary>
        private bool InitInfoPanel(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("No info panel prefab provided");
                return false;
            }
            prefab.transform.SetParent(_mainCanvas.transform, false);//IMPORTANT: second argument is "false" to allow for canvas rescaling on different screens.
            _currentInfoPanel = prefab.GetComponent<ElementInfoPanel>();
            if (_currentInfoPanel == null)
            {
                Debug.LogError("Could not find info panel on prefab");
                return false;
            }
            _currentInfoPanel.Initialize(_selectedElement);

            //subscribe to events.
            //start modify
            _currentInfoPanel.StartModifyTool.OnStartModify += StartModifying;
            //cancel modify
            _currentInfoPanel.ModifyPanel._closeTool.OnClose += CancelCommit;
            //commit modify
            _currentInfoPanel.ModifyPanel.CommitTool.OnCommit += CommitUpdates;
            //modification events from tools
            foreach(var t in _currentInfoPanel.ModifyTools)
            {
                t.OnElementModified += ElementModify;
            }
            //removal tool
            _currentInfoPanel.RemoveTool.OnSelectedElementRemoved += RemoveSelectedElement;    
            //deselect tool
            _currentInfoPanel.DeselectTool.OnDeselect += DeselectElement;
            //scene change
            if (_currentInfoPanel is CityInfoPanel)
            {
                var cP = _currentInfoPanel as CityInfoPanel;
                cP.GoToCity.OnSceneChange += ChangeScene;
            }
            return true;
        }

        /// <summary>
        /// Called when deselecting a scene element.
        /// </summary>
        protected virtual void DeselectElement(object sender, DeselectArgs args)
        {
            if (_selectedElement != null)
            {
                _selectedElement.SetSelectedState(false);
                _selectedElement = null;
            }

            if (_currentInfoPanel != null)
            {
                _currentInfoPanel.gameObject.Destroy();
            }
        }

        #endregion

        #region MODIFY ELEMENTS

        /// <summary>
        /// Main method to call when making modifications to objects in scene.
        /// </summary>
        protected abstract void ElementModify(IModifyElementArgs args);

        /// <summary>
        /// Called when we start modifying a scene element.
        /// </summary>
        protected void StartModifying(object sender, System.EventArgs args)
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
            else if (_selectedElement is SceneCity)
            {
                var sC = _selectedElement as SceneCity;
                var specs = new CityOptions(sC.CitySpecs);
                _workingCopy = InstantiateCity(guid, specs, false);
            }

            _selectedElement.SetActive(false); //hide original element
            _workingCopy.SetSelectedState(true);
        }

        /// <summary>
        /// Called when we commit modification of an element.
        /// </summary>
        protected void CommitUpdates(object sender, System.EventArgs args)
        {
            EndModification(true);
        }

        /// <summary>
        /// Called when we cancel modification of an element.
        /// </summary>
        protected void CancelCommit(object sender, System.EventArgs args)
        {
            EndModification(false);
        }

        /// <summary>
        /// Called when we finish modification of an element.
        /// </summary>
        protected void EndModification(bool commit)
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
                    _selectedElement = InstantiateDronePort(guidNew, wC.DronePortSpecs, true);
                }
                else if (_workingCopy is SceneParkingStructure)
                {
                    var wC = _workingCopy as SceneParkingStructure;
                    RemoveParkingStructure(guidOld);
                    EnvironManager.Instance.AddParkingStructure(guidNew, wC.ParkingStructureSpecs);
                    _selectedElement = InstantiateParkingStructure(guidNew, wC.ParkingStructureSpecs, true);
                }
                else if (_workingCopy is SceneRestrictionZone)
                {
                    var wC = _workingCopy as SceneRestrictionZone;
                    RemoveRestrictionZone(guidOld);
                    EnvironManager.Instance.AddRestrictionZone(guidNew, wC.RestrictionZoneSpecs);
                    _selectedElement = InstantiateRestrictionZone(guidNew, wC.RestrictionZoneSpecs, true);
                }
                else if (_workingCopy is SceneCity)
                {
                    //cities are special case because city object at region level is still existing, but we want to upate specs...
                    //a bit confusing because other objects behave differently.
                    var wc = _workingCopy as SceneCity;
                    if (Cities.ContainsKey(guidOld))
                    {
                        Cities[guidOld].gameObject.Destroy();
                        Cities.Remove(guidOld);
                    }
                    EnvironManager.Instance.GetCities()[guidOld].CityStats = wc.CitySpecs;
                    _selectedElement = InstantiateCity(guidOld, wc.CitySpecs, true);
                }
            }

            //regardless, we want to delete this working copy, if there is one.
            _workingCopy?.gameObject.Destroy();
            _workingCopy = null;

            //update selected because we threw out old
            _selectedElement.SetActive(true);
            _selectedElement.SetSelectedState(true);
        }

        /// <summary>
        /// Modifies the working drone port port
        /// </summary>
        protected void DronePortUpdate(IModifyElementArgs args)
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
                switch (args.Update.ElementPropertyType)
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
                    case ElementPropertyType.Position:
                        {
                            dP.Position = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.Rotation:
                        {
                            dP.Rotation = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.Scale:
                        {
                            dP.Scale = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.XScale:
                        {
                            dP.Scale = new Vector3((args.Update as ModifyFloatPropertyArg).Value, dP.Scale.y, dP.Scale.z);
                            break;
                        }
                    case ElementPropertyType.ZScale:
                        {
                            dP.Scale = new Vector3(dP.Scale.x, dP.Scale.y, (args.Update as ModifyFloatPropertyArg).Value);
                            break;
                        }
                    case ElementPropertyType.StandByPos:
                        {
                            dP.StandbyPosition = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.LandingQueueHead:
                        {
                            dP.LandingQueueHead = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.LandingQueueDirection:
                        {
                            dP.LandingQueueDirection = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.LandingPoint:
                        {
                            dP.LandingPoint = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.MaxVehicleSize:
                        {
                            dP.MaximumVehicleSize = (args.Update as ModifyFloatPropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.IsMountable:
                        {
                            dP.IsMountable = (args.Update as ModifyBoolPropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.IsOnTheGround:
                        {
                            dP.IsOnTheGround = (args.Update as ModifyBoolPropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.IsScalable:
                        {
                            dP.IsScalable = (args.Update as ModifyBoolPropertyArg).Value;
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
        protected void ParkingStructureUpdate(IModifyElementArgs args)
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
                switch (args.Update.ElementPropertyType)
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
                    case ElementPropertyType.Position:
                        {
                            pS.Position = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.Rotation:
                        {
                            pS.Rotation = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.Scale:
                        {
                            pS.Scale = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.StandByPos:
                        {
                            pS.StandbyPosition = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.LandingQueueHead:
                        {
                            pS.LandingQueueHead = (args.Update as ModifyVector3PropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.LandingQueueDirection:
                        {
                            pS.LandingQueueDirection = (args.Update as ModifyVector3PropertyArg).Value;
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
        protected void RestrictionZoneUpdate(IModifyElementArgs args)
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
        protected abstract void AddElement(IAddElementArgs args);

        /// <summary>
        /// Removes any type of element from scene.
        /// </summary>
        protected virtual void RemoveSelectedElement(object sender, System.EventArgs args)
        {
            if (_selectedElement == null)
            {
                Debug.LogError("Nothing selected");
                return;
            }
            if (_selectedElement is SceneDronePort)
            {
                RemoveDronePort((_selectedElement as SceneDronePort).Guid);
            }
            else if (_selectedElement is SceneParkingStructure)
            {
                RemoveParkingStructure((_selectedElement as SceneParkingStructure).Guid);
            }
            else if (_selectedElement is SceneRestrictionZone)
            {
                RemoveRestrictionZone((_selectedElement as SceneRestrictionZone).Guid);
            }
            else if (_selectedElement is SceneCity)
            {
                RemoveCity((_selectedElement as SceneCity).Guid);
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
                            dP = EnvironManager.Instance.DronePortAssets[args.Type].Specs.GetCopy();
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
                            pS = EnvironManager.Instance.ParkingStructAssets[args.Type].Specs.GetCopy();
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
        /// Adds city to game and to environment
        /// </summary>
        protected void AddNewCity(AddCityArgs args)
        {
            var gO = args.HitInfo.transform.gameObject;
            var uT = gO.GetComponent<UnityTile>();
            string guid = Guid.NewGuid().ToString();
            var p = args.Position;

            var tileBounds = Conversions.TileBounds(uT.UnwrappedTileId);
            var regionTileWorldCenter = uT.gameObject.transform.position;
            CityOptions s = new CityOptions();
            s.WorldPos = p;
            s.RegionTileWorldCenter = regionTileWorldCenter;
            InstantiateCity(guid, s, true);
            EnvironManager.Instance.AddCity(guid, new City(s));
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
                DronePorts[guid].gameObject.Destroy();
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
                ParkingStructures[guid].gameObject.Destroy();
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
                RestrictionZones[guid].gameObject.Destroy();
                RestrictionZones.Remove(guid);
            }
            else
            {
                Debug.LogError("Restriction zone for removal not found in scene dictionary");
            }
        }

        /// <summary>
        /// Removes city from region.
        /// </summary>
        protected void RemoveCity(string guid)
        {
            EnvironManager.Instance.RemoveCity(guid);
            if (Cities.ContainsKey(guid))
            {
                Cities[guid].OnSceneElementSelected -= SelectElement;
                Cities[guid].gameObject.Destroy();
                //var cM = m.GetComponentInChildren<CityMarker>();
                //if (cM != null)
                //{
                //    cM._markerSelected -= CityMarkerSelected;
                //}
                Cities.Remove(guid);
                //m.Destroy();
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

        /// <summary>
        /// Places a marker at a city.
        /// </summary>
        protected abstract SceneCity InstantiateCity(string guid, CityOptions cityOptions, bool register);

        #endregion

        #region SIMULATION FUNCTIONS

        public void GenerateNetwork()
        {

            const float UTM_ELEVATION = 152.4f;
            const float UTM_SEPARATION = 25.0f;


            List<GameObject> points = new List<GameObject>();
            foreach (SceneDronePort sdp in DronePorts.Values) points.Add(sdp.gameObject);
            foreach (SceneParkingStructure sps in ParkingStructures.Values)
            {
                if ( !sps.ParkingStructureSpecs.Type.Contains("LowAltitude") ) points.Add(sps.gameObject);
            }

            if (points.Count < 3) return;

            IPoint[] vertices = GetVertices(points);
            Delaunator delaunay = new Delaunator(vertices);
            network = new Assets.Scripts.DataStructure.Network();
            network.vertices = points;
            for (int i = 0; i < delaunay.Triangles.Length; i++)
            {
                if (i > delaunay.Halfedges[i])
                {
                    GameObject from = points[delaunay.Triangles[i]];
                    GameObject to = points[delaunay.Triangles[nextHalfEdge(i)]];
                    Corridor corridor_from_to = new Corridor(from, to, UTM_ELEVATION);
                    Vector3 fromFlat = new Vector3(from.transform.position.x, 0, from.transform.position.z);
                    Vector3 toFlat = new Vector3(to.transform.position.x, 0, to.transform.position.z);
                    Vector3 fromPosition = fromFlat + Vector3.Normalize(toFlat - fromFlat) * 50;
                    Vector3 toPosition = toFlat + Vector3.Normalize(fromFlat - toFlat) * 50;
                    Vector3 corridor_from_to_start = new Vector3(fromPosition.x, UTM_ELEVATION, fromPosition.z);
                    Vector3 corridor_from_to_end = new Vector3(toPosition.x, UTM_ELEVATION, toPosition.z);



                    corridor_from_to_start.y = UTM_ELEVATION;
                    corridor_from_to_end.y = UTM_ELEVATION;

                    Corridor corridor_to_from = new Corridor(to, from, UTM_ELEVATION + UTM_SEPARATION);
                    Vector3 corridor_to_from_start = new Vector3(toPosition.x, UTM_ELEVATION + UTM_SEPARATION, toPosition.z);
                    Vector3 corridor_to_from_end = new Vector3(fromPosition.x, UTM_ELEVATION + UTM_SEPARATION, fromPosition.z);

                    var wayPoints = FindPath(corridor_from_to_start, corridor_from_to_end, 5, 1 << 9 | 1 << 8);
                    wayPoints.Insert(0, corridor_from_to_start);
                    corridor_from_to.wayPoints = new Queue<Vector3>(wayPoints);

                    wayPoints = FindPath(corridor_to_from_start, corridor_to_from_end, 5, 1 << 9 | 1 << 8);
                    wayPoints.Insert(0, corridor_to_from_start);
                    corridor_to_from.wayPoints = new Queue<Vector3>(wayPoints);

                    network.corridors.Add(corridor_from_to);
                    network.corridors.Add(corridor_to_from);
                    if (!network.inEdges.ContainsKey(from)) network.inEdges.Add(from, new List<Corridor>());
                    if (!network.outEdges.ContainsKey(from)) network.outEdges.Add(from, new List<Corridor>());
                    if (!network.inEdges.ContainsKey(to)) network.inEdges.Add(to, new List<Corridor>());
                    if (!network.outEdges.ContainsKey(to)) network.outEdges.Add(to, new List<Corridor>());

                    network.outEdges[from].Add(corridor_from_to);
                    network.inEdges[from].Add(corridor_to_from);
                    network.outEdges[to].Add(corridor_to_from);
                    network.inEdges[to].Add(corridor_from_to);
                }
            }

        }

        public int nextHalfEdge(int e)
        {
            return (e % 3 == 2) ? e - 2 : e + 1;
        }
        public IPoint[] GetVertices(List<GameObject> points)
        {
            List<IPoint> pts = new List<IPoint>();
            foreach (GameObject gO in points)
            {
                Vector3 position = gO.transform.position;
                pts.Add(new Point((double)position.x, (double)position.z));
            }
            return pts.ToArray();
        }

        RaycastHit GetClosestHit(Vector3 current, RaycastHit[] hit)
        {
            float minDist = Mathf.Infinity;
            RaycastHit minHit = hit[0];
            foreach (RaycastHit h in hit)
            {
                if (Vector3.Distance(current, h.point) < minDist)
                {
                    minHit = h;
                    minDist = Vector3.Distance(current, h.point);
                }
            }
            return minHit;
        }

        Vector3 RotateAround(Vector3 point, Vector3 pivot, float angle)
        {
            Vector3 newPoint = point - pivot;
            newPoint = Quaternion.Euler(0, angle, 0) * newPoint;
            newPoint = newPoint + pivot;
            return newPoint;
        }
        public List<Vector3> FindPath(Vector3 origin, Vector3 destination, int angleIncrement, int layerMask )
        {
            // For pathfinding, omit drone colliders

            int head = 0, tail = 0;
            List<Vector3> visited = new List<Vector3>();
            List<int> from = new List<int>();
            List<float> distance = new List<float>();
            visited.Add(origin);
            from.Add(-1);
            distance.Add(0.0f);
            tail++;

            while (Physics.Raycast(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask) && head <= tail)
            {
                RaycastHit currentHitObject = GetClosestHit(visited[head], Physics.RaycastAll(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination), layerMask));
                Vector3 lastHit = currentHitObject.point;
                for (int i = angleIncrement; i <= 85; i += angleIncrement)
                {
                    Vector3 currentVector = destination - visited[head];
                    RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination), layerMask);
                    if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                    {
                        Vector3 newWaypoint = RotateAround(lastHit, visited[head], (float)angleIncrement);
                        visited.Add(newWaypoint);
                        from.Add(head);
                        distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                        tail++;
                        break;
                    }
                    else
                    {
                        lastHit = hit[0].point;
                    }
                }

                // Do the same thing in the opposite direction
                lastHit = currentHitObject.point;
                for (int i = angleIncrement; i <= 85; i += angleIncrement)
                {
                    Vector3 currentVector = destination - visited[head];
                    RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, -i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination), layerMask);
                    if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                    {
                        Vector3 newWaypoint = RotateAround(lastHit, visited[head], -(float)angleIncrement);
                        visited.Add(newWaypoint);
                        from.Add(head);
                        distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                        tail++;
                        break;
                    }
                    else
                    {
                        lastHit = hit[0].point;
                    }
                }

                head++;
            }
            // Do the same thing in the opposite direction
            List<Vector3> path = new List<Vector3>();
            if (head < tail) // found a path so backtrack
            {
                if (head > 0) path = Backtrack(visited, from, distance, head);
                path.Add(destination);
            }
            else // cannot find a path - just fly straight
            {
                //path.Add(origin);
                path.Add(destination);
            }

            if (path.Count == 0) path.Add(destination);
            return path;
        }

        List<Vector3> Backtrack(List<Vector3> visited, List<int> from, List<float> distance, int head)
        {
            List<Vector3> result = new List<Vector3>();
            int pointer = head;
            do
            {
                result.Add(visited[pointer]);
                pointer = from[pointer];
            } while (pointer >= 0 && from[pointer] != -1);
            //if (pointer != -1) result.Add(visited[pointer]);
            result.Reverse();
            return result;
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

        public int GetParkingCapacity(string type)
        {
            int parking_capacity = 0;
            foreach (var kvp in ParkingStructures)
            {
                if(kvp.Value.ParkingStructureSpecs.Type.Contains(type)) parking_capacity += kvp.Value.ParkingStructureSpecs.ParkingSpots.Count;
            }
            return parking_capacity;
        }

        #endregion

        #region AIRSPACE 

        /// <summary>
        /// Gets airspace data from our tileset on Mapbox 
        /// </summary>
        protected void GetAirspaceData(object sender, System.EventArgs args)
        {
            if (_largeScaleMap != null)
            {
                EnvironManager.Instance.Environ.AirspaceTiles = new Dictionary<string, AirspaceTile>();

                UnityTile[] tiles = _largeScaleMap.GetComponentsInChildren<UnityTile>(true);
                foreach (var t in tiles)
                {
                    EnvironManager.Instance.DownloadAirspace(t);
                }
            }
        }

        /// <summary>
        /// Instantiates airspace elements if we have them.
        /// </summary>
        protected void BuildAirspace()
        {
            //instantiate airspace.
            foreach (var aT in EnvironManager.Instance.Environ.AirspaceTiles.Values)
            {
                foreach (var f in aT.Features.Values)
                {
                    f.CreateGeometry(_airspaceYScale);
                }
            }
        }

        #endregion

    }
}
