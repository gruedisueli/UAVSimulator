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
    public abstract class SceneIconBase : MonoBehaviour
    {
        public Color _selectedColor;
        public Color _defaultColor;

        public bool IsSelected { get; protected set; } = false;

        protected ElementFollower _follower;
        protected Button _button;
        protected Image _sprite;
        public EventHandler<System.EventArgs> OnSelected;

        protected virtual void Awake()
        {
            _sprite = GetComponentInChildren<Image>(true);
            if (_sprite == null)
            {
                Debug.LogError("Icon sprite not specified");
                return;
            }
            _button = GetComponentInChildren<Button>(true);
            if (_button == null)
            {
                Debug.LogError("Button Component not found in children of icon");
                return;
            }
            _button.onClick.AddListener(OnPointerUp);
            _follower = GetComponentInChildren<ElementFollower>(true);
            if (_follower == null)
            {
                Debug.LogError("Could not find Element Follower Component in children of scene icon");
                return;
            }
            if (_selectedColor == null)
            {
                Debug.LogError("Selected color of icon not specified");
                return;
            }
            if (_defaultColor == null)
            {
                Debug.LogError("Default color of icon not specified");
            }

            SetSelected(false);
        }

        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// Pointer up assumed as selection event.
        /// </summary>
        public virtual void OnPointerUp()
        {
            SetSelected(true);
            OnSelected?.Invoke(this, new System.EventArgs());
        }

        /// <summary>
        /// Used to set icon in selected or deselected state.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            _sprite.color = isSelected ? _selectedColor : _defaultColor;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
