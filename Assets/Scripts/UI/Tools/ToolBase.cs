using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.UI;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public abstract class ToolBase : MonoBehaviour
    {
        /// <summary>
        /// If there is a button, or some other selectable component on this game object, this should be populated. Otherwise will be null.
        /// </summary>
        public Selectable SelectableElement { get; private set; } = null;

        protected virtual void Awake()
        {
            SelectableElement = GetComponent<Selectable>();
        }

        /// <summary>
        /// If the game object has a selectable component, sets it accordingly, else does nothing.
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            if (SelectableElement != null)
            {
                SelectableElement.interactable = isInteractable;
            }
        }
    }
}
