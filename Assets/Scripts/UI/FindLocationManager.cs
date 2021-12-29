using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Mapbox.Examples;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.Tags;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI
{
    public class FindLocationManager : MonoBehaviour
    {
        public GameObject _regionSelectorPrefab;
        public Canvas _canvas;
        private GameObject _selectionRect;

        private AbstractMap _map;
        private AddRegionTool _addRegionTool;

        private List<GameObject> _selectedTiles = new List<GameObject>();
        private Color _emissionColor = new Color(0.5f, 0.5f, 0.5f);

        private void Start()
        {
            _map = FindObjectOfType<AbstractMap>(true);
            if (_map == null)
            {
                Debug.LogError("Abstract map not found");
                return;
            }
            _map.SetZoom(EnvironSettings.FINDLOCATION_ZOOM_LEVEL);

            _addRegionTool = FindObjectOfType<AddRegionTool>(true);
            if (_addRegionTool == null)
            {
                Debug.LogError("Add region tool not found");
                return;
            }

            _addRegionTool.ElementAddedEvent += PlaceMarker;
        }

        private void OnDestroy()
        {
            _addRegionTool.ElementAddedEvent -= PlaceMarker;
        }

        public void CreateProject()
        {
            StartCoroutine(LoadAsyncOperation(UISettings.REGIONVIEW_SCENEPATH));
        }

        public void PlaceMarker(IAddElementArgs args)
        {
            if (args is AddRegionArgs)
            {
                var a = args as AddRegionArgs;
                var gO = a.HitInfo.transform.gameObject;
                string name = gO.name;
                string d = "/";
                var frags = name.Split(new string[] { d }, StringSplitOptions.None);
                Vector3 center = gO.transform.position;
                if (frags != null && frags.Length == 3)
                {
                    foreach(var t in _selectedTiles)
                    {
                        var mR = t.GetComponent<MeshRenderer>();
                        if (mR != null)
                        {
                            mR.material.DisableKeyword("_EMISSION");
                        }
                    }
                    _selectedTiles = new List<GameObject>();
                    string zLevel = frags[0];
                    int rI = EnvironSettings.FINDLOCATION_SELECTION_INFLATION;
                    bool haveX = int.TryParse(frags[1], out int xTile);
                    bool haveY = int.TryParse(frags[2], out int yTile);
                    List<string> toHighlight = new List<string>() { name };
                    if (haveX && haveY)
                    {
                        for (int x = xTile - rI; x <= xTile + rI; x++)
                        {
                            for (int y = yTile - rI; y <= yTile + rI; y++)
                            {
                                if (x == xTile && y == yTile)//already have this tile
                                {
                                    continue;
                                }

                                toHighlight.Add(zLevel + d + x.ToString() + d + y.ToString());
                            }
                        }
                    }
                    
                    foreach(var n in toHighlight)
                    {
                        var foundTile = GameObject.Find(n);
                        if (foundTile == null)
                        {
                            Debug.LogError("Could not find specified tile");
                            continue;
                        }
                        var mR = foundTile.GetComponent<MeshRenderer>();
                        if (mR != null)
                        {
                            mR.material.EnableKeyword("_EMISSION");
                            mR.material.SetColor("_EmissionColor", _emissionColor);
                        }

                        _selectedTiles.Add(foundTile);
                    }
                }
                EnvironManager.Instance.Environ.CenterLatLong = _map.WorldToGeoPosition(center);


            }

        }

        //private IEnumerator PlaceMarkerCoroutine()
        //{
        //    #region ALLOW CLICK FROM BUTTON TO CLEAR OUT

        //    while (!Input.GetMouseButtonUp(0) && !Input.GetKeyUp(KeyCode.Escape))
        //    {
        //        yield return null;
        //    }

        //    if (Input.GetMouseButtonUp(0)) //after initial button press
        //    {
        //        yield return null;
        //    }


        //    #endregion

        //    if (_regionSelectorPrefab != null)
        //    {
        //        var p0 = Input.mousePosition;
        //        if (_selectionRect != null)
        //        {
        //            _selectionRect.Destroy();
        //        }
        //        _selectionRect = Instantiate(_regionSelectorPrefab);
        //        _selectionRect.transform.localScale = new Vector3(_regionSize, 1000, _regionSize);
        //        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyUp(KeyCode.Escape))
        //        {
        //            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
        //            if (hit)
        //            {
        //                _selectionRect.transform.position = hitInfo.point;
        //            }

        //            yield return null;
        //        }

        //        var p = MarkPt();

        //        if (p != null)
        //        {
        //            Vector2d latLong = _map.WorldToGeoPosition((Vector3)p);
        //            EnvironManager.Instance.Environ.CenterLatLong = latLong;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Marks a point in space. Returns world coordinates or null on failure.
        ///// </summary>
        //private Vector3? MarkPt()
        //{
        //    bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
        //    if (hit)
        //    {
        //        var gO = hitInfo.transform.gameObject;
        //        if (gO.GetComponent<UnityTile>() == null) //check that we've hit a terrain tile and not something else like a button.
        //        {
        //            Debug.Log("No hit");
        //            return null;
        //        }
        //    }
        //    return hitInfo.point;
        //}

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
