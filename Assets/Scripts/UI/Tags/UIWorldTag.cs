using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tags
{
    public abstract class UIWorldTag : MonoBehaviour
    {
        protected Camera _mainCamera;
        protected RectTransform _rectTransform;
        protected Vector3 _worldPos = new Vector3();
        public bool _followOn = true;

        private void Start()
        {
            _mainCamera = Camera.main;
            _rectTransform = GetComponent<RectTransform>();
            CustomStart();
        }

        private void Update()
        {
            if (_followOn)
            {
                if (_mainCamera != null && _rectTransform != null)
                {
                    var p = _mainCamera.WorldToScreenPoint(_worldPos);
                    _rectTransform.anchoredPosition = new Vector2(p.x, p.y);
                }
            }
            CustomUpdate();
        }

        protected virtual void CustomUpdate()
        {

        }

        protected virtual void CustomStart()
        {

        }

        public void SetWorldPos(Vector3 p)
        {
            _worldPos = p;
        }

    }
}
