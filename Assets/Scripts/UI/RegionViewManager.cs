using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;

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
        private CityPanel _cityPanel; //CONSIDER JUST GETTING ARRAY ON BASE CLASS.

        protected override void Init()
        {
            string rPath = "GUI/";
            _cityMarkerPrefab = AssetUtils.ReadPrefab(rPath, "CityMarker");
            if (_cityMarkerPrefab == null)
            {
                Debug.LogError("City marker prefab not found");
                return;
            }
            _abstractMap.Initialize(EnvironManager.Instance.Environ.CenterLatLong, EnvironSettings.REGION_ZOOM_LEVEL);

            _cityPanel = FindObjectOfType<CityPanel>(true);
            if (_cityPanel == null)
            {
                Debug.LogError("Modify city panel not found");
                return;
            }

            _cityPanel.OnCloseCityPanel += DeselectElement;

            _cityPanel.SetActive(false);
        }

        protected override void OnDestroyDerived()
        {
            _cityPanel.OnCloseCityPanel -= DeselectElement;
        }


        #region SELECTION

        protected override bool SelectElement(SceneElementBase sE)
        {
            bool selectedNew = base.SelectElement(sE);

            if (selectedNew)
            {
                SelectCityMarker();
                SetCityPanelActive(true);
            }

            return selectedNew;
        }

        private void SelectCityMarker()
        {
            var guid = _selectedElement.Guid;
            if (_cityPanel != null)
            {
                var city = EnvironManager.Instance.GetCity(guid);
                if (city != null)
                {
                    _cityPanel.SetCity(city.CityStats);

                }
                //if (_cityMarkers.ContainsKey(guid))
                //{
                //    var cM = _cityMarkers[guid].GetComponentInChildren<CityMarker>();
                //    if (cM != null)
                //    {
                //        cM.SetColor(Color.red);
                //        selectedMarker = cM;
                //    }
                //}
            }

        }

        private void SetCityPanelActive(bool isActive)
        {
            if (_cityPanel != null)
            {
                _cityPanel.SetActive(isActive);
            }
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

        protected override void RemoveSelectedElement()
        {
            base.RemoveSelectedElement();

            SetCityPanelActive(false);
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
        }

        protected override SceneDronePort InstantiateCustomDronePort(string guid, GameObject prefab, DronePortCustom dP, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneParkingStructure InstantiateCustomParkingStruct(string guid, GameObject prefab, ParkingStructureCustom pS, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneDronePort InstantiateDronePort(string guid, DronePortBase dP, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneParkingStructure InstantiateParkingStructure(string guid, ParkingStructureBase pS, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneDronePort InstantiateRectDronePort(string guid, DronePortRect dP, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneParkingStructure InstantiateRectParkingStruct(string guid, ParkingStructureRect pS, bool register)
        {
            throw new NotImplementedException();
        }

        protected override SceneRestrictionZone InstantiateRestrictionZone(string guid, RestrictionZoneBase rZ, bool register)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Places a marker at a city.
        /// </summary>
        protected override SceneCity InstantiateCity(string guid, CityOptions cityOptions, bool register)
        {
            var c = cityOptions;

            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            var sCity = clone.AddComponent<SceneCity>();
            sCity.Initialize(guid, c);

            sCity.OnSceneElementSelected += SelectElement;


            //var m = Instantiate(_cityMarkerPrefab);
            //var cM = m.GetComponentInChildren<CityMarker>();
            //if (cM != null)
            //{
            //    cM.SetWorldPos(selectedPt);
            //    cM.SetName(n);
            //    cM.SetGuid(guid);
            //    cM._markerSelected += CityMarkerSelected;
            //    var tile = GetLocalCityTileCoords(selectedPt, regionTileWorldCenter, cityTileSideLength);
            //    var extents = TileCoordsToWorldExtents(tile, regionTileWorldCenter, cityTileSideLength, eastExt, westExt, northExt, southExt);
            //    cM.SetExtents(extents);

            //}


            if (register)
            {
                Cities.Add(guid, sCity);
            }

            return sCity;
        }

        #endregion


        #region MODIFICATION

        protected override void ElementModify(IModifyElementArgs args)
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
                    switch (args.Update.Type)
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

            _workingCopy.UpdateGameObject();
        }

        ///// <summary>
        ///// Updates a city
        ///// </summary>
        //private void UpdateCityStats(string guid, CityOptions stats)
        //{
        //    //City city = EnvironManager.Instance.Environ.GetCity(guid);
        //    var city = EnvironManager.Instance.GetCity(guid);
        //    if (city != null)
        //    {
        //        city.CityStats = stats;
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    if (_cityMarkers.ContainsKey(guid))
        //    {
        //        var cM = _cityMarkers[guid].GetComponentInChildren<CityMarker>();
        //        if (cM != null)
        //        {
        //            cM.SetName(stats.Name);
        //            float cityTileSide = (float)GetCityTileSideLength();
        //            var tile = GetLocalCityTileCoords(city.CityStats.WorldPos, city.CityStats.RegionTileWorldCenter, cityTileSide);
        //            var extents = TileCoordsToWorldExtents(tile, city.CityStats.RegionTileWorldCenter, cityTileSide, stats.EastExt, stats.WestExt, stats.NorthExt, stats.SouthExt);
        //            cM.SetExtents(extents);
        //        }
        //    }
        //}

        #endregion
    }
}
