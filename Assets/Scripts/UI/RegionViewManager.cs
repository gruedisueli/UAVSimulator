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

        private Coroutine _reloadRoutine;
        private WaitForSeconds _wait;
        private CameraAdjustment _cameraAdj;

        private GameObject _cityMarkerPrefab;

        private float _minCamSize = 1000.0f;
        private float _maxCamSize = 256000.0f;
        private float _biggestZoom = 15;
        //private float _smallestZoom = 7;
        private float _currentZoom;
        private bool _atBuildingZoomLevel = false;//activated whenever we get to a zoom level that we could possibly display buildings at without enormous processing cost.
        private bool _allowBuildings = false;//true if user toggle for buildings is in "on" position. Only if true can we show buildings.
        private bool _temporarySuppressBuildings = false;//set true of just holding off buildings for a second while changing view.
        private VectorSubLayerProperties _buildingsLayer = null;

        //zoom levels:
        //500
        //1000
        //2000
        //4000
        //8000
        //16000
        //32000
        //64000
        //128000
        //256000

        private float GetZoomLevel(float viewSize)
        {
            float z = _currentZoom;
            if (/*viewSize > _minCamSize &&*/ viewSize < _maxCamSize)
            {
                float m = viewSize / _minCamSize;//how many times bigger than min?
                z = _biggestZoom - (float)Math.Log(m, 2); //how many powers of two higher?

                //z = _biggestZoom - (viewSize / _maxCamSize) * (_biggestZoom - _smallestZoom);
            }

            return z;
        }

        protected override void Init()
        {
            string rPath = "GUI/";
            _cityMarkerPrefab = AssetUtils.ReadPrefab(rPath, "CityMarker");
            if (_cityMarkerPrefab == null)
            {
                Debug.LogError("City marker prefab not found");
                return;
            }

            //RangeTileProviderOptions range = new RangeTileProviderOptions();
            //range.east = EnvironSettings.REGION_TILE_EXTENTS;
            //range.west = EnvironSettings.REGION_TILE_EXTENTS;
            //range.north = EnvironSettings.REGION_TILE_EXTENTS;
            //range.south = EnvironSettings.REGION_TILE_EXTENTS;
            //_abstractMap.SetExtentOptions(range);
            _abstractMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.REGION_ZOOM_LEVEL);
            _largeScaleMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.AIRSPACE_ZOOM_LEVEL);

            //if (EnvironManager.Instance.LastCamXZS != null)
            //{
            //    var p = EnvironManager.Instance.LastCamXZS;
            //    _mainCamera.transform.position = new Vector3(p[0], _mainCamera.transform.position.y, p[1]);
            //    _mainCamera.orthographicSize = p[2];
            //}

            _cameraAdj = FindObjectOfType<CameraAdjustment>(true);
            if (_cameraAdj == null)
            {
                Debug.LogError("Camera Adjustment component not found");
                return;
            }

            _currentZoom = _abstractMap.AbsoluteZoom;
            _cameraAdj.OnZoom += OnZoomAction;
            _cameraAdj.OnStartViewChange += OnStartViewChangeAction;
            _cameraAdj.OnEndViewChange += OnEndViewChangeAction;

            _buildingsLayer = _abstractMap.VectorData.FindFeatureSubLayerWithName("Buildings");
            if (_buildingsLayer == null)
            {
                Debug.LogError("Failed to find buildings feature layer");
                return;
            }

            #region from Mapbox ReloadMap.cs

            _wait = new WaitForSeconds(0.3f);

            #endregion
        }

        protected override void OnDestroyDerived()
        {
            //_abstractMap.OnTileFinished -= PullAirspaceOffMapbox;
        }

        private void OnZoomAction(object o, System.EventArgs args)
        {
            if (!(o is CameraAdjustment cA)) return;
            var z = GetZoomLevel(cA._camera.transform.position.y);
            Reload(z);
        }

        private void OnStartViewChangeAction(object o, System.EventArgs args)
        {
            if (_allowBuildings)
            {
                _temporarySuppressBuildings = true;
                SetAllowBuildings(false);
            }
        }

        private void OnEndViewChangeAction(object o, System.EventArgs args)
        {
            if (_temporarySuppressBuildings)
            {
                _temporarySuppressBuildings = false;
                SetAllowBuildings(true);
            }
        }

        private void SetAllowBuildings(bool toggle)
        {
            _allowBuildings = toggle;
            if (_atBuildingZoomLevel && !_allowBuildings)
            {
                _buildingsLayer.SetActive(false);
            }
            else if (_atBuildingZoomLevel && _allowBuildings)
            {
                _buildingsLayer.SetActive(true);
            }
        }


        #region copied from Mapbox: ReloadMap.cs

        public void Reload(float zoom)
        {
            if (_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }
            _reloadRoutine = StartCoroutine(ReloadAfterDelay(zoom));
        }

        IEnumerator ReloadAfterDelay(float zoom)
        {
            yield return _wait;
            //_camera.transform.position = _cameraStartPos;
            if (zoom >= _biggestZoom && !_atBuildingZoomLevel)
            {
                if (_allowBuildings)
                {
                    _buildingsLayer.SetActive(true);
                }
                _atBuildingZoomLevel = true;
            }
            else if (zoom < _biggestZoom && _atBuildingZoomLevel)
            {
                _buildingsLayer.SetActive(false);
                _atBuildingZoomLevel = false;
            }
            _abstractMap.UpdateMap(_abstractMap.CenterLatitudeLongitude, zoom);
            _reloadRoutine = null;
            _currentZoom = zoom;
        }

        #endregion

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
                        case VisibilityType.Buildings:
                            {
                                var u = args.Update as ModifyBoolPropertyArg;
                                SetAllowBuildings(u.Value);
                                break;
                            }
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
                _workingCopy.UpdateGameObject();
            }

        }

        #endregion
    }

}
