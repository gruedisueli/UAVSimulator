using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.EventSystems;
using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class SelectableGameObject : MonoBehaviour
    {
        public EventHandler<SelectGameObjectArgs> OnSelected;
        private CameraAdjustment _cameraAdjustment;

        public void Awake()
        {
            _cameraAdjustment = FindObjectOfType<CameraAdjustment>(true);
            if (_cameraAdjustment == null)
            {
                Debug.LogError("Could not find camera adjustment script in the scene");
            }
        }

        public void OnMouseUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && !_cameraAdjustment.IsTiltingCamera && !_cameraAdjustment.IsPanningCamera)
            {
                Debug.Log("Clicked selectable game object");
                OnSelected?.Invoke(this, new SelectGameObjectArgs());
            }
            else
            {
                Debug.Log("Game object behind UI");
            }
        }
    }
}
