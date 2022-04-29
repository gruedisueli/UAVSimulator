using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Scripts.UI.Tools;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.Environment
{
    public delegate bool SceneElementSelected(SceneElementBase element);

    public abstract class SceneElementBase : MonoBehaviour
    {
        public abstract string Guid { get; protected set; }
        public abstract GameObject Sprite2d { get; protected set; }
        public abstract Canvas SceneCanvas { get; protected set; }
        public abstract void UpdateGameObject();
        public event SceneElementSelected OnSceneElementSelected;
        protected Material _selectedMaterial;
        protected Material _defaultMaterial;
        protected MeshRenderer[] _renderers;
        protected SelectableGameObject[] _selectableObjs;

        private void Awake()
        {
            //if (IsSelectable)
            //{
            //    MakeSelectable();
            //}

            if (_renderers == null)//some scene elements may define this before "Start" is called.
            {
                _renderers = GetComponentsInChildren<MeshRenderer>(true);
                if (_renderers == null)
                {
                    Debug.LogError("Mesh renderers not found on scene element");
                }
            }

            if (_selectedMaterial == null)
            {
                _selectedMaterial = Instantiate(EnvironManager.Instance.SelectedSceneElementMat);
            }
            if (_defaultMaterial == null)
            {
                _defaultMaterial = Instantiate(EnvironManager.Instance.DefaultSceneElementMat);
            }
        }

        private void OnDestroy()
        {
            Sprite2d.Destroy();
            if (_selectableObjs == null || _selectableObjs.Length == 0) return;
            foreach (var o in _selectableObjs)
            {
                o.OnSelected -= Selected;
            }
        }

        public void Selected(object sender, SelectGameObjectArgs args)
        {
            if (EnvironManager.Instance.VCS.playing) return;//prevent modification of scene elements when playing.
            if (!EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                Debug.Log("Clicked scene element " + Guid);
                OnSceneElementSelected?.Invoke(this);
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public virtual void SetSelectedState(bool isSelected)
        {
            foreach(var r in _renderers)
            {
                r.material = isSelected ? _selectedMaterial : _defaultMaterial;
            }
        }

        protected void MakeSelectable()
        {
            gameObject.AddComponent<SelectableGameObject>();

            var c = gameObject.GetComponent<Collider>();
            if (c is BoxCollider)
            {
                var bC = c as BoxCollider;
                bC.size = new Vector3(2, 2, 2);//bigger seems to solve raycast issues
            }

            _selectableObjs = GetComponentsInChildren<SelectableGameObject>(true);
            if (_selectableObjs == null)
            {
                Debug.LogError("Selectable game object component not found in this scene element or its children");
                return;
            }
            foreach (var o in _selectableObjs)
            {
                o.OnSelected += Selected;
            }
        }

        protected void SpriteClicked()
        {
            if (EnvironManager.Instance.VCS.playing) return;//prevent modification of scene elements when playing.
            Debug.Log("Clicked scene element " + Guid);
            OnSceneElementSelected?.Invoke(this);
        }
    }
}
