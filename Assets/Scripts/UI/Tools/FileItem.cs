using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Tools
{
    public class FileItem : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public EventHandler<System.EventArgs> OnSelected;
        public EventHandler<System.EventArgs> OnDeselected;
        private Text _text;

        private void Awake()
        {
            _text = GetComponentInChildren<Text>();
        }

        /// <summary>
        /// Called when selecting button
        /// </summary>
        public void OnSelect(BaseEventData data)
        {
            OnSelected?.Invoke(this, new System.EventArgs());
        }

        /// <summary>
        /// Called when deselecting button
        /// </summary>
        public void OnDeselect(BaseEventData data)
        {
            OnDeselected?.Invoke(this, new System.EventArgs());
        }

        /// <summary>
        /// Initialize this button.
        /// </summary>
        public void Init(string name)
        {
            _text.text = name;
        }

        /// <summary>
        /// Returns name.
        /// </summary>
        public string GetName()
        {
            return _text.text;
        }
    }
}
