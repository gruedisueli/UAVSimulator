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
        private DroneInfoPanel _droneInfoPanel;
        private List<DroneIcon> _droneIcons = new List<DroneIcon>();

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

            _droneInfoPanel = FindObjectOfType<DroneInfoPanel>(true);
            if (_droneInfoPanel == null)
            {
                Debug.LogError("Drone Info Panel not found");
                return;
            }
            _droneInfoPanel.SetActive(false);

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

            _vehicleControlSystem.droneInstantiator.OnDroneInstantiated += OnDroneInstantiated;
        }

        protected override void OnDestroyDerived()
        {
            foreach(var i in _droneIcons)
            {
                if (i != null)
                {
                    i.OnSelected -= DroneSelected;
                }
            }

            _vehicleControlSystem.droneInstantiator.OnDroneInstantiated -= OnDroneInstantiated;
        }

        #region MODIFICATION

        /// <summary>
        /// Main method to call when making modifications to objects in scene.
        /// </summary>
        protected override void ElementModify(IModifyElementArgs args)
        {
            if (args.Update.Category == ToolMessageCategory.VisibilityModification)
            {
                try
                {
                    switch (args.Update.VisibilityType)
                    {
                        case VisibilityType.DroneCount:
                            {
                                var u = args.Update as ModifyIntPropertyArg;
                                _vehicleControlSystem.UpdateVehicleCount(u.Value);
                                break;
                            }
                        case VisibilityType.Demographics:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.ToggleDemographicVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.FlightTrails:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.ToggleTrailVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.LandingCorridors:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.ToggleLandingCorridorVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.Noise:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.ToggleNoiseVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.Privacy:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.TogglePrivacyVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.Routes:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.ToggleRouteVisualization(u.Value);
                                break;
                            }
                        case VisibilityType.NoiseSpheres:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.noiseShpereVisualization = u.Value;
                                break;
                            }
                        case VisibilityType.VehicleMeshSimple:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                _vehicleControlSystem.simplifiedMeshToggle = u.Value;
                                break;
                            }
                        case VisibilityType.RestrictionZones:
                            {
                                //see: https://answers.unity.com/questions/348974/edit-camera-culling-mask.html
                                var u = args.Update as ModifyBoolPropertyArg;
                                LayerVisibility("RestrictionZone", u.Value);
                                LayerVisibility("LandingZone", u.Value);
                                break;
                            }
                    }
                }
                catch
                {
                    Debug.LogError("Casting error in visiblity modification");
                    return;
                }
            }
            else
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

        protected override void InstantiateObjects()
        {
            InstantiateSimulationObjects(true, false);
        }

        #endregion

        #region SIMULATION

        /// <summary>
        /// Called whenever a drone is added to scene.
        /// </summary>
        protected void OnDroneInstantiated(object sender, DroneInstantiationArgs args)
        {
            var gO = args.Drone;

            var clone = Instantiate(EnvironManager.Instance.DroneIconPrefab);
            clone.transform.SetParent(_mainCanvas.transform, false);
            var icon = clone.GetComponentInChildren<DroneIcon>(true);
            if (icon == null)
            {
                Debug.LogError("Could not find Drone Icon Component in Drone Info Panel prefab");
                return;
            }
            var drone = gO.GetComponentInChildren<DroneBase>(true);
            if (drone == null)
            {
                Debug.LogError("Could not find Vehicle Component in children of drone");
                return;
            }

            icon.Initialize(drone);

            icon.OnSelected += DroneSelected;

            _droneIcons.Add(icon);
        }

        /// <summary>
        /// Called when a drone icon is selected.
        /// </summary>
        protected void DroneSelected(object sender, System.EventArgs args)
        {
            SceneIconBase icon = sender as SceneIconBase;
            if (icon == null)
            {
                Debug.LogError("Sender of drone selection message is not an icon");
                return;
            }
            _droneInfoPanel.Initialize(icon.gameObject);
        }

        #endregion

    }
}
