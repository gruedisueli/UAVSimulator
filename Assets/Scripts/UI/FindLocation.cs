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

using Assets.Scripts.UI.Tags;
using Assets.Scripts.Environment;

namespace Assets.Scripts.UI
{
    public class FindLocation : MonoBehaviour
    {
        public AbstractMap _map;
        public GameObject _selectionRectPrefab;
        private GameObject _selectionRect;

        private Vector3 _worldPt0 = new Vector3();
        private Vector3 _worldPt1 = new Vector3();

        public void CreateProject()
        {
            SceneManager.LoadScene(UISettings.REGIONVIEW_SCENEPATH, LoadSceneMode.Single);
        }

        public void PlaceMarker()
        {
            StartCoroutine(PlaceMarkerCoroutine());
        }

        private IEnumerator PlaceMarkerCoroutine()
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

            while (!Input.GetMouseButtonDown(0) && !Input.GetKeyUp(KeyCode.Escape))
            {
                yield return null;
            }

            #endregion

            if (_selectionRectPrefab != null)
            {
                var wP0 = MarkPt(0);
                if (wP0 != null)
                {
                    var p0 = Input.mousePosition;
                    if (_selectionRect != null)
                    {
                        _selectionRect.Destroy();
                    }
                    _selectionRect = Instantiate(_selectionRectPrefab);
                    var sR = _selectionRect.GetComponentInChildren<SelectRect>();
                    sR._followOn = false;
                    if (sR != null)
                    {
                        while (!Input.GetMouseButtonUp(0) && !Input.GetKeyUp(KeyCode.Escape))
                        {
                            sR.SetExtents(p0, Input.mousePosition);
                            yield return null;
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            var wP1 = MarkPt(1);
                            if (wP1 != null)
                            {
                                _worldPt0 = (Vector3)wP0;
                                _worldPt1 = (Vector3)wP1;
                                var c = (_worldPt0 + _worldPt1) / 2;
                                SetEnviroCenter(_worldPt0, _worldPt1);
                                sR.SetWorldPos(c);
                                sR._followOn = true;
                            }
                        }                
                    }
                }
            }
        }

        /// <summary>
        /// Marks a point in space. Returns world coordinates or null on failure.
        /// </summary>
        private Vector3? MarkPt(int index)
        {
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hit)
            {
                var gO = hitInfo.transform.gameObject;
                if (gO.GetComponent<UnityTile>() == null) //check that we've hit a terrain tile and not something else like a button.
                {
                    Debug.Log("No hit");
                    return null;
                }
            }
            return hitInfo.point;
        }

        /// <summary>
        /// Sets the center of the simulation.
        /// </summary>
        private void SetEnviroCenter(Vector3 p0, Vector3 p1)
        {
            var center = (p0 + p1) / 2;
            Vector2d latLong = _map.WorldToGeoPosition(center);
            EnvironController.Instance.Environ.centerLatLong = latLong;
        }
    }
}
