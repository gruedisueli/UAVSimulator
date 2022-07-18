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
        public static bool TryToSelect(out RaycastHit? hitInfo)
        {
            int layerMask = 1 << 14 | 1 << 9;//14=ground, 9=building
            var r = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 1000000, layerMask);
            hitInfo = null;
            if (r == null || r.Length == 0)
            {
                Debug.Log("No hit");
                return false;
            }

            hitInfo = ViewportUtils.GetClosestHit(Camera.main.transform.position, r);
            return true;
        }
    }
}
