using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class ZoomSelected : MonoBehaviour
    {
        private SceneManagerBase _sceneManager;
        private CameraAdjustment _cameraAdjustment;

        private void Awake()
        {
            _sceneManager = FindObjectOfType<SceneManagerBase>(true);
            if (_sceneManager == null)
            {
                Debug.LogError("Could not find scene manager for the zoom selected tool");
                return;
            }

            _cameraAdjustment = FindObjectOfType<CameraAdjustment>(true);
            if (_cameraAdjustment == null)
            {
                Debug.LogError("Could not find camera adjustment script for zoom selected tool");
                return;
            }
        }

        public void ZoomSel()
        {
            var selElem = _sceneManager.SelectedElement;
            if (selElem == null)
            {
                Debug.Log("No scene element selected");
                return;
            }

            _cameraAdjustment.ZoomToPosition(selElem.transform.position);
        }

    }
}
