using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// A simple UI behavior for following a game object in screen space.
    /// </summary>
    public class ElementFollower : MonoBehaviour
    {
        protected Camera _mainCamera = null;
        protected GameObject _sceneElement = null;

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("Main camera not found for element follower to reference");
            }
        }

        public void Initialize(GameObject target)
        {
            _sceneElement = target;
        }

        private void Update()
        {
            if (_sceneElement != null && _mainCamera != null)
            {
                var rT = gameObject.GetComponent<RectTransform>();
                var sPt = ViewportUtils.WorldPtCanvasPoint(_sceneElement.transform.position, _mainCamera);
                rT.position = new Vector3(sPt.x, sPt.y, 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
