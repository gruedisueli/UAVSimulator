using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;

using Assets.Scripts.Environment;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI.Tags;
using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Manages the region view. Only functions specific to region view should be here. If it can be generalized, put it in the base class.
    /// </summary>
    public class RegionViewManager : SceneManagerBase
    {

        private GameObject _cityMarkerPrefab;
        //private List<UnityTile> _tiles = new List<UnityTile>();
        protected override void Init()
        {
            string rPath = "GUI/";
            _cityMarkerPrefab = AssetUtils.ReadPrefab(rPath, "CityMarker");
            if (_cityMarkerPrefab == null)
            {
                Debug.LogError("City marker prefab not found");
                return;
            }

            RangeTileProviderOptions range = new RangeTileProviderOptions();
            range.east = EnvironSettings.REGION_TILE_EXTENTS;
            range.west = EnvironSettings.REGION_TILE_EXTENTS;
            range.north = EnvironSettings.REGION_TILE_EXTENTS;
            range.south = EnvironSettings.REGION_TILE_EXTENTS;
            _abstractMap.SetExtentOptions(range);
            _abstractMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.REGION_ZOOM_LEVEL);
            _largeScaleMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.AIRSPACE_ZOOM_LEVEL);
        }

        protected override void OnDestroyDerived()
        {
            //_abstractMap.OnTileFinished -= PullAirspaceOffMapbox;
        }

        #region ADD/REMOVE ELEMENTS

        protected override void AddElement(IAddElementArgs args)
        {
            if (args is AddCityArgs)
            {
                AddNewCity(args as AddCityArgs);
            }
        }

        #endregion

        #region INSTANTIATION

        protected override void InstantiateObjects()
        {

            foreach (var kvp in EnvironManager.Instance.GetCities())
            {
                var c = kvp.Value;
                if (c != null)
                {
                    InstantiateCity(kvp.Key, c.CityStats, true);
                }
            }

            InstantiateSimulationObjects(false, true);
        }

        #endregion

        #region MODIFICATION

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
                if (_workingCopy is SceneCity)
                {
                    var sC = _workingCopy as SceneCity;
                    try
                    {
                        switch (args.Update.ElementPropertyType)
                        {
                            case ElementPropertyType.Name:
                                {
                                    sC.CitySpecs.Name = (args.Update as ModifyStringPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.Population:
                                {
                                    sC.CitySpecs.Population = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.Jobs:
                                {
                                    sC.CitySpecs.Jobs = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.EastExt:
                                {
                                    sC.CitySpecs.EastExt = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.WestExt:
                                {
                                    sC.CitySpecs.WestExt = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.NorthExt:
                                {
                                    sC.CitySpecs.NorthExt = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                            case ElementPropertyType.SouthExt:
                                {
                                    sC.CitySpecs.SouthExt = (args.Update as ModifyIntPropertyArg).Value;
                                    break;
                                }
                        }

                    }
                    catch
                    {
                        Debug.LogError("Casting error in working city modification");
                        return;
                    }
                }
            }

            _workingCopy.UpdateGameObject();
        }

        #endregion
    }

}
