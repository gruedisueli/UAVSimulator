using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tags
{
    /// <summary>
    /// A point in the UI that is tagged to a piece of 3d geometry. Useful for rendering elements that track 3d objects but should be in the UI layer in front of everything else.
    /// </summary>
    public class UIWorldPt
    {
        private Camera _mainCamera = null;
        public Vector3 _worldPos { get; private set; } = new Vector3();

        public UIWorldPt()
        {

        }

        public UIWorldPt(Vector3 worldPos)
        {
            _mainCamera = Camera.main;
            _worldPos = worldPos;
        }

        public Vector3 GetScreenPt()
        {
            if (_mainCamera != null)
            {
                //var p = ViewportUtils.WorldPtCanvasPoint(_worldPos, _mainCamera);
                var p = _mainCamera.WorldToScreenPoint(_worldPos);
                //if (Input.GetKey(KeyCode.L))
                //{
                //    Debug.Log($"world pos: {_worldPos.x}, {_worldPos.y}, {_worldPos.z}");
                //    Debug.Log($"screen pos: {p.x}, {p.y}, {p.z}");
                //    Debug.Log($"screen pos2: {p2.x}, {p2.y}, {p2.z}");
                //    Debug.Log($"Camera size: {_mainCamera.pixelWidth} x {_mainCamera.pixelHeight}");
                //    Debug.Log($"Camera scaled size: {_mainCamera.scaledPixelWidth} x {_mainCamera.scaledPixelHeight}");
                //    var mP = Input.mousePosition;
                //    Debug.Log($"Mouse Pos: {mP.x}, {mP.y}");

                //}
                return p;
            }
            else return new Vector3();
        }

    }
}
