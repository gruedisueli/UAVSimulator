using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public abstract class PanelBase : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnPanelClosed;
        public CloseTool _closeTool;

        protected virtual void Awake()
        {
            //note: we are explicitly requiring assignment of close tools to panels because there may be multiples in the entire tree structure for the game object.
            //yes, it's a slight inconvenience to wire up in the editor, but potentially saves us debugging headaches.
            if (_closeTool != null)
            {
                _closeTool.OnClose += Close;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_closeTool != null)
            {
                _closeTool.OnClose -= Close;
            }
        }

        /// <summary>
        /// Toggles visibility of this panel.
        /// </summary>
        public void ToggleActive()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        protected virtual void Close(object sender, System.EventArgs args)
        {
            SetActive(false);
            OnPanelClosed?.Invoke(this, new System.EventArgs());
        }

        public virtual void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        /// <summary>
        /// Easy way to find single component in children and flag problems. Null on failure.
        /// </summary>
        protected T FindCompInChildren<T>()
        {
            var comp = GetComponentInChildren<T>(true);
            if (comp == null)
            {
                Debug.Log($"Could not find child component {typeof(T).ToString()} on UI panel");
            }

            return comp;
        }
    }
}
