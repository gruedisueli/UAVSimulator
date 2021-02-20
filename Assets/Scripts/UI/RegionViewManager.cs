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
using Assets.Scripts.UI.Panels;

namespace Assets.Scripts.UI
{
    public class RegionViewManager : SceneManagerBase
    {
        public AbstractMap abstractMap;
        public GameObject cityMarkerPrefab;
        public GameObject markCityPanel;
        public RegionRight regionRight;
        public SavePrompt _savePrompt;

        /// <summary>
        /// City markers in region, keyed by guid.
        /// </summary>
        private Dictionary<string, GameObject> cityMarkers = new Dictionary<string, GameObject>();
        private CityMarker selectedMarker;

        protected override void Init()
        {
            abstractMap.Initialize(EnvironManager.Instance.Environ.centerLatLong, EnvironSettings.REGION_ZOOM_LEVEL);
            foreach(var guid in EnvironManager.Instance.Environ.GetCityGuids())
            {
                var c = EnvironManager.Instance.Environ.GetCity(guid);
                if (c != null)
                {
                    float cityTileSide = (float)GetCityTileSideLength();
                    InstantiateCityMarker(c._regionTileWorldCenter, cityTileSide, c._worldPos, c._cityStats._name, guid, c._cityStats._eastExt, c._cityStats._westExt, c._cityStats._northExt, c._cityStats._southExt);
                }
            }
            if (regionRight != null)
            {
                regionRight.statsChanged += UpdateCityStats;
            }
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


        public void GoToCity()
        {
            EnvironManager.Instance.SetActiveCity(selectedMarker._guid);
            SceneManager.LoadScene(UISettings.CITYVIEW_SCENEPATH, LoadSceneMode.Single);
        }

        /// <summary>
        /// Adds city to game and to environment
        /// </summary>
        public void AddCity()
        {
            var inputF = markCityPanel?.GetComponentInChildren<InputField>();

            if (inputF == null)
            {
                return;
            }
            StartCoroutine(PlaceCityCoroutine(inputF.text));
        }

        /// <summary>
        /// Turns on the mark city panel
        /// </summary>
        public void ShowHideMarkCityPanel()
        {
            if (markCityPanel == null)
            {
                return;
            }

            bool isEnabled = markCityPanel.activeSelf;
            markCityPanel.SetActive(!isEnabled);
        }

        public void CityMarkerSelected(string guid, string cityName)
        {
            if (selectedMarker != null)
            {
                DeselectMarker();
            }

            if (regionRight != null)
            {
                regionRight.Activate();
                var city = EnvironManager.Instance.Environ.GetCity(guid);
                if (city != null)
                {
                    regionRight.SetCity(guid, city._cityStats);

                }
                if (cityMarkers.ContainsKey(guid))
                {
                    var cM = cityMarkers[guid].GetComponentInChildren<CityMarker>();
                    if (cM != null)
                    {
                        cM.SetColor(Color.red);
                        selectedMarker = cM;
                    }
                }
            }

        }

        public void CloseRightPanel()
        {
            if (regionRight != null)
            {
                regionRight.Deactivate();
            }
        }

        public void DeselectMarker()
        {
            if (selectedMarker != null)
            {
                selectedMarker.ResetColor();
            }
            selectedMarker = null;
        }

        /// <summary>
        /// Removes city from region.
        /// </summary>
        public void RemoveCity()
        {
            string g = regionRight?._guid;
            EnvironManager.Instance.Environ.RemoveCity(g);
            if (cityMarkers.ContainsKey(g))
            {
                selectedMarker = null;
                GameObject m = cityMarkers[g];
                var cM = m.GetComponentInChildren<CityMarker>();
                if (cM != null)
                {
                    cM._markerSelected -= CityMarkerSelected;
                }
                cityMarkers.Remove(g);
                m.Destroy();
            }

            CloseRightPanel();
        }

        private IEnumerator PlaceCityCoroutine(string n)
        {
            #region ALLOW CLICK FROM BUTTON TO CLEAR OUT

            while (!Input.GetMouseButtonUp(0) && !Input.GetKeyUp(KeyCode.Escape))
            {
                yield return null;
            }

            if (Input.GetMouseButtonUp(0)) //after initial button press
            {
                yield return null;
            }

            #endregion

            while (!Input.GetMouseButtonUp(0) && !Input.GetKeyUp(KeyCode.Escape))
            {
                yield return null;
            }
            if (Input.GetMouseButtonUp(0))
            {
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
                if (hit)
                {
                    var gO = hitInfo.transform.gameObject;
                    var uT = gO.GetComponent<UnityTile>();
                    if (uT != null) //check that we've hit a terrain tile and not something else like a button.
                    {
                        var p = hitInfo.point;
                        string guid = Guid.NewGuid().ToString();
                        var tileBounds = Conversions.TileBounds(uT.UnwrappedTileId);
                        float regionTileSideLength = (float)tileBounds.Size.x;
                        float cityTileSide = (float)GetCityTileSideLength();
                        var regionTileWorldCenter = uT.gameObject.transform.position;
                        InstantiateCityMarker(regionTileWorldCenter, cityTileSide, p, n, guid);
                        CityOptions s = new CityOptions();
                        s._name = n;
                        EnvironManager.Instance.Environ.AddCity(guid, new City(p, gO.transform.position, regionTileSideLength, s));
                    }
                    else
                    {
                        hit = false;
                    }
                }

                if (!hit)
                {
                    Debug.Log("No hit");
                }

            }
        }

        /// <summary>
        /// Gets the length of a city tile side.
        /// See https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale 
        /// </summary>
        private double GetCityTileSideLength()
        {
            double lat = EnvironManager.Instance.Environ.centerLatLong.x;
            int zoom = EnvironSettings.CITY_ZOOM_LEVEL;
            int res = EnvironSettings.TILE_RESOLUTION;
            double c = EnvironSettings.SCALE_CONSTANT;
            return c * Math.Cos(lat) / Math.Pow(2, zoom) * res;
        }

        /// <summary>
        /// Places a marker at a city.
        /// </summary>
        private void InstantiateCityMarker(Vector3 regionTileWorldCenter, float cityTileSideLength, Vector3 selectedPt, string n, string guid, int eastExt = 0, int westExt = 0, int northExt = 0, int southExt = 0)
        {
            var m = Instantiate(cityMarkerPrefab);
            var cM = m.GetComponentInChildren<CityMarker>();
            if (cM != null)
            {
                cM.SetWorldPos(selectedPt);
                cM.SetName(n);
                cM.SetGuid(guid);
                cM._markerSelected += CityMarkerSelected;
                var tile = GetLocalCityTileCoords(selectedPt, regionTileWorldCenter, cityTileSideLength);
                var extents = TileCoordsToWorldExtents(tile, regionTileWorldCenter, cityTileSideLength, eastExt, westExt, northExt, southExt);
                cM.SetExtents(extents);

            }
            cityMarkers.Add(guid, m);
        }

        /// <summary>
        /// Updates a city
        /// </summary>
        private void UpdateCityStats(string guid, CityOptions stats)
        {
            City city = EnvironManager.Instance.Environ.GetCity(guid);
            if (city != null)
            {
                city._cityStats = stats;
            }
            else
            {
                return;
            }

            if (cityMarkers.ContainsKey(guid))
            {
                var cM = cityMarkers[guid].GetComponentInChildren<CityMarker>();
                if (cM != null)
                {
                    cM.SetName(stats._name);
                    float cityTileSide = (float)GetCityTileSideLength();
                    var tile = GetLocalCityTileCoords(city._worldPos, city._regionTileWorldCenter, cityTileSide);
                    var extents = TileCoordsToWorldExtents(tile, city._regionTileWorldCenter, cityTileSide, stats._eastExt, stats._westExt, stats._northExt, stats._southExt);
                    cM.SetExtents(extents);
                }
            }
        }


        /// <summary>
        /// Finds the local tile coordinates (x,y) within the larger region tile where provided world coordinate is located. X,Y coords are relative to region tile center.
        /// </summary>
        private int[] GetLocalCityTileCoords(Vector3 sampleWorldPos, Vector3 regionTileWorldCenter, float cityTileSide)
        {

            //since side factor is a power of two, the center of the region tile will be on the border of four city tiles
            Vector3 toSample = sampleWorldPos - regionTileWorldCenter;
            int xSteps = (int)Math.Ceiling(toSample.x / cityTileSide);
            int ySteps = (int)Math.Ceiling(toSample.z / cityTileSide);
            return new int[] { xSteps, ySteps };
        }

        /// <summary>
        /// Converts city tile coordinates within a region into actual world coordinates: x-range, z-range
        /// </summary>
        /// <returns></returns>
        private float[][] TileCoordsToWorldExtents(int[] coordsXY, Vector3 regionTileWorldCenter, float cityTileSideLength, int eExt, int wExt, int nExt, int sExt)
        {
            int x1 = coordsXY[0] + wExt;
            int x0 = coordsXY[0] - 1 - eExt;
            int y1 = coordsXY[1] + sExt;
            int y0 = coordsXY[1] - 1 - nExt;

            float xR0 = CoordToWorldCoord(regionTileWorldCenter.x, x0, cityTileSideLength);
            float xR1 = CoordToWorldCoord(regionTileWorldCenter.x, x1, cityTileSideLength);
            float zR0 = CoordToWorldCoord(regionTileWorldCenter.z, y0, cityTileSideLength);
            float zR1 = CoordToWorldCoord(regionTileWorldCenter.z, y1, cityTileSideLength);

            return new float[][] { new float[] { xR0, xR1 }, new float[] { zR0, zR1 } };

        }

        /// <summary>
        /// Converts a single tile division line coord (int) into position in world space based on relevant region center tile coordinate compoent.
        /// </summary>
        private float CoordToWorldCoord(float worldCenterCoord, int coord, float cityTileSideLength)
        {
            return worldCenterCoord + coord * cityTileSideLength;
        }
    }
}
