using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Environment
{
    public delegate void SceneElementSelected(SceneElementBase element);

    public abstract class SceneElementBase : MonoBehaviour
    {
        public abstract string Guid { get; protected set; }
        public abstract void UpdateGameObject();
        public event SceneElementSelected OnSceneElementSelected;

        private void Start()
        {
            gameObject.AddComponent<BoxCollider>();//for UI selection
        }

        private void OnMouseUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                Debug.Log("Clicked scene element " + Guid);
                OnSceneElementSelected.Invoke(this);
            }
        }
    }
}
