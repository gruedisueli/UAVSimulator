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

        }

        protected override void OnDestroyDerived()
        {
            
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
