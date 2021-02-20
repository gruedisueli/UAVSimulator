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
using Assets.Scripts.UI.Panels;

namespace Assets.Scripts.UI
{
    public class CityView : MonoBehaviour
    {
        public AbstractMap abstractMap;
        public FOA_RangeAroundTransformTileProvider _tileProvider;
        public Material selectedMaterial;
        public GameObject _cityCenter;
        public Camera _mainCamera;
        public SavePrompt _savePrompt;

        private List<Tuple<GameObject, Material, Material[], Vector3>> selectedObjects = new List<Tuple<GameObject, Material, Material[], Vector3>>();

        public void Start()
        {
            var eC = EnvironController.Instance;
            var city = eC.Environ.GetCity(eC.ActiveCity);
            if (city != null)
            {
                _cityCenter.transform.position = city._worldPos;
                var options = new FOA_RangeAroundTransformTileProviderOptions();
                options._eastExt = city._cityStats._eastExt;
                options._westExt = city._cityStats._westExt;
                options._northExt = city._cityStats._northExt;
                options._southExt = city._cityStats._southExt;
                options._targetTransform = _cityCenter.transform;
                _tileProvider.SetOptions(options);
                _mainCamera.transform.position = new Vector3(city._worldPos.x, 1000, city._worldPos.z);
                abstractMap.Initialize(EnvironController.Instance.Environ.centerLatLong, EnvironSettings.CITY_ZOOM_LEVEL);
            }
        }

        private void LateUpdate()
        {
            //if (Input.GetMouseButtonUp(0)) //try to select something
            //{
            //    if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))//deselect previous
            //    {
            //        Deselect();
            //    }
            //    Select();
            //}

            //if (Input.GetKeyUp(KeyCode.D) && (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)))
            //{
            //    AddDronePorts();
            //    Deselect();
            //}

            //if (Input.GetKeyUp(KeyCode.S) && (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)))
            //{
            //    SaveFile();
            //}
        }

        /// <summary>
        /// Loads up the region view of current environment
        /// </summary>
        public void GoToRegion()
        {
            SceneManager.LoadScene(UISettings.REGIONVIEW_SCENEPATH, LoadSceneMode.Single);
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

        private void Deselect()
        {
            for (int s = selectedObjects.Count - 1; s >= 0; s--)
            {
                var sO = selectedObjects[s];
                var mR = sO.Item1.GetComponent<MeshRenderer>();
                if (mR != null)
                {
                    mR.sharedMaterial = sO.Item2;
                    mR.sharedMaterials = sO.Item3;
                }
                selectedObjects.RemoveAt(s);
            }
        }

        private void Select()
        {

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hit)
            {
                var gO = hitInfo.transform.gameObject;
                Debug.Log("Hit " + gO.name);
                //if (gO.name.Contains("Buildings"))
                //{
                //    Debug.Log("Building Selected");
                //}
                //else
                //{
                //    Debug.Log("Not a building");
                //}
                var mR = gO.GetComponent<MeshRenderer>();
                Material mainMat = mR.sharedMaterial;
                Material[] mats = mR != null ? mR.sharedMaterials : new Material[0];
                var p = hitInfo.point;
                selectedObjects.Add(Tuple.Create(gO, mainMat, mats, p));
                if (mR != null)
                {
                    mR.sharedMaterial = selectedMaterial;
                    mR.sharedMaterials = new Material[] { selectedMaterial, selectedMaterial };
                }
            }
            else
            {
                Debug.Log("No hit");
            }

        }

        //private void AddDronePorts()
        //{
        //    foreach (var sO in selectedObjects)
        //    {
        //        environController.AddDronePort(sO.Item4, 0);
        //    }
        //}


    }
}
