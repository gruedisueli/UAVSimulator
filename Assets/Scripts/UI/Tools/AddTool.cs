using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.UI;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementAdded(IAddElementArgs args);

    /// <summary>
    /// Behavior for adding elements to scene
    /// </summary>
    public abstract class AddTool : ToolBase
    {
        protected RaycastHit _hitInfo;
        protected Vector3 _position;
        public event ElementAdded ElementAddedEvent;

        private void Start()
        {
            Initialize();
        }

        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Cancel()
        {
            gameObject.SetActive(false);
        }

        public virtual void Add()
        {
            ActivateMouseHint();
            StartCoroutine(PlaceAtLocationCoroutine());
        }

        protected abstract void Initialize();
        protected abstract IAddElementArgs GatherInformation();
        protected abstract void ActivateMouseHint();
        
        private IEnumerator PlaceAtLocationCoroutine()
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
                if (GUIUtils.TryToSelect(out var hitInfo))
                {
                    _hitInfo = (RaycastHit)hitInfo;
                    _position = _hitInfo.point;
                    ElementAddedEvent.Invoke(GatherInformation());
                }
            }

            EnvironManager.Instance.CanvasMouseHint.Deactivate();
        }
    }
}
