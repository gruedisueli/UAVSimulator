using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mapbox.Unity.Utilities;
using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class AddCityTool : AddTool
    {
        private RaycastHit _hitInfo;
        private Vector3 _position;

        protected override void Initialize()
        {
            
        }

        protected override IAddElementArgs GatherInformation()
        {
            return new AddCityArgs(_position, _hitInfo);
        }

        public void AddNewCity()
        {
            StartCoroutine(PlaceCityCoroutine());
        }

        private IEnumerator PlaceCityCoroutine()
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
                        _hitInfo = hitInfo;
                        _position = hitInfo.point;

                        Add();
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
    }
}
