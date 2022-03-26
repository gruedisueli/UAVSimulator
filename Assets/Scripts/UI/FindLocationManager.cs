using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using Mapbox.Examples;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.Tags;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FindLocationManager : MonoBehaviour
    {
        public Canvas _canvas;
        public Button _createProjectButton;

        private AbstractMap _map;

        private float _tileSize;
        private float _regionTileSize;
        private GameObject _placedRegion = null;

        private void Start()
        {
            _map = FindObjectOfType<AbstractMap>(true);
            if (_map == null)
            {
                Debug.LogError("Abstract map not found");
                return;
            }
            _map.SetZoom(EnvironSettings.FINDLOCATION_ZOOM_LEVEL);
            _tileSize = _map.Options.scalingOptions.unityTileSize;
            _regionTileSize = _tileSize / (float)Math.Pow(2, EnvironSettings.REGION_ZOOM_LEVEL - EnvironSettings.FINDLOCATION_ZOOM_LEVEL);


        }

        private void OnDestroy()
        {
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {

            }
            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject() && GUIUtils.TryToSelect(out var hitInfo))
            {
                PlaceMarker(hitInfo);
            }
        }

        /// <summary>
        /// Returns to main menu, canceling find location operation
        /// </summary>
        public void BackToMain()
        {
            StartCoroutine(LoadAsyncOperation(UISettings.MAINMENU_SCENEPATH));
        }

        public void CreateProject()
        {
            StartCoroutine(LoadAsyncOperation(UISettings.REGIONVIEW_SCENEPATH));
        }

        public void PlaceMarker(RaycastHit hitInfo)
        {
            if (_placedRegion != null)
            {
                _placedRegion.Destroy();
            }
            //highlight region extents in the find location view.
            //get center of a tile used at region scale.
            //requires converting from find location scale to region scale tiles.
            var gO = hitInfo.transform.gameObject;
            var mainPt = new Vector3(hitInfo.point.x, 0, hitInfo.point.z);
            var gOXMin = gO.transform.position.x - (_tileSize / 2);
            var gOZMin = gO.transform.position.z - (_tileSize / 2);
            var subX = (float)Math.Floor((mainPt.x - gOXMin) / _regionTileSize);
            var subZ = (float)Math.Floor((mainPt.z - gOZMin) / _regionTileSize);
            var trueCenter = new Vector3(gOXMin + subX * _regionTileSize + _regionTileSize / 2, 0, gOZMin + subZ * _regionTileSize + _regionTileSize / 2);
            var infDist = EnvironSettings.REGION_TILE_EXTENTS * _regionTileSize + _regionTileSize / 2;
            _placedRegion = Instantiate(AssetUtils.ReadPrefab("GUI/", "RegionBox"));
            _placedRegion.transform.SetParent(_canvas.transform);
            _placedRegion.transform.SetAsFirstSibling();
            var rB = _placedRegion.GetComponent<RegionBox>();
            rB.SetWorldExtents(trueCenter, infDist);

            EnvironManager.Instance.Environ.CenterLatLong = _map.WorldToGeoPosition(trueCenter);

            _createProjectButton.interactable = true;


            //string name = gO.name;
            //string d = "/";
            //var frags = name.Split(new string[] { d }, StringSplitOptions.None);
            //Vector3 center = gO.transform.position;
            //if (frags != null && frags.Length == 3)
            //{
            //    foreach(var t in _selectedTiles)
            //    {
            //        var mR = t.GetComponent<MeshRenderer>();
            //        if (mR != null)
            //        {
            //            mR.material.SetFloat("_FirstOutlineWidth", 0.0f);
            //        }
            //    }
            //    _selectedTiles = new List<GameObject>();
            //    string zLevel = frags[0];
            //    int rI = EnvironSettings.FINDLOCATION_SELECTION_INFLATION;
            //    bool haveX = int.TryParse(frags[1], out int xTile);
            //    bool haveY = int.TryParse(frags[2], out int yTile);
            //    List<string> toHighlight = new List<string>() { name };
            //    if (haveX && haveY)
            //    {
            //        for (int x = xTile - rI; x <= xTile + rI; x++)
            //        {
            //            for (int y = yTile - rI; y <= yTile + rI; y++)
            //            {
            //                if (x == xTile && y == yTile)//already have this tile
            //                {
            //                    continue;
            //                }

            //                toHighlight.Add(zLevel + d + x.ToString() + d + y.ToString());
            //            }
            //        }
            //    }

            //    foreach(var n in toHighlight)
            //    {
            //        var foundTile = GameObject.Find(n);
            //        if (foundTile == null)
            //        {
            //            Debug.LogError("Could not find specified tile");
            //            continue;
            //        }
            //        var mR = foundTile.GetComponent<MeshRenderer>();
            //        if (mR != null)
            //        {
            //            mR.material.SetFloat("_FirstOutlineWidth", 10.0f);
            //        }

            //        _selectedTiles.Add(foundTile);
            //    }
            //}
            //EnvironManager.Instance.Environ.CenterLatLong = _map.WorldToGeoPosition(center);
        }

        IEnumerator LoadAsyncOperation(int scenePath)
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

    }
}
