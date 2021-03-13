using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Environment
{
    public delegate bool SceneElementSelected(SceneElementBase element);

    public abstract class SceneElementBase : MonoBehaviour
    {
        public abstract string Guid { get; protected set; }
        public abstract void UpdateGameObject();
        public event SceneElementSelected OnSceneElementSelected;

        private void Start()
        {
            //disabled because it seems like primitives come with colliders, and we can assign colliders when we instantiate the game objects if they are custom//gameObject.AddComponent<BoxCollider>();//for UI selection

            var bC = gameObject.GetComponent<BoxCollider>();
            bC.size = new Vector3(2, 2, 2);//bigger seems to solve raycast issues
        }

        private void OnMouseUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                Debug.Log("Clicked scene element " + Guid);
                OnSceneElementSelected.Invoke(this);
            }
        }

        public abstract void SetSelectedState(bool isSelected);
    }
}
