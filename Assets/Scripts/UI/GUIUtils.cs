using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mapbox.Unity.MeshGeneration.Data;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public static class GUIUtils
    {
        /// <summary>
        /// Tries to select a point for adding elements on an existing UnityTile. Returns true if hit something we want, else false.
        /// </summary>
        public static bool TryToSelect(out RaycastHit hitInfo)
        {
            Debug.Log("Firing");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                Debug.Log("raycast");
                var gO = hitInfo.transform.gameObject;
                var uT = gO.GetComponentInParent<UnityTile>();
                if (uT != null) //check that we've hit a terrain tile and not something else like a button.
                {
                    Debug.Log("Hit");
                    return true;
                }
            }
            Debug.Log("No hit");
            return false;
        }
    }
}
