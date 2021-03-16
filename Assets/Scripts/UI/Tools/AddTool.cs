using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementAdded(IAddElementArgs args);

    /// <summary>
    /// Behavior for adding elements to scene
    /// </summary>
    public abstract class AddTool : MonoBehaviour
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
            StartCoroutine(PlaceAtLocationCoroutine());
        }

        protected abstract void Initialize();
        protected abstract IAddElementArgs GatherInformation();
        
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
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
                if (hit)
                {
                    var gO = hitInfo.transform.gameObject;
                    var uT = gO.GetComponent<UnityTile>();
                    if (uT != null) //check that we've hit a terrain tile and not something else like a button.
                    {
                        _hitInfo = hitInfo;
                        _position = hitInfo.point;

                        ElementAddedEvent.Invoke(GatherInformation());
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
