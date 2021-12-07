using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tags
{
    public class UIWorldTag : MonoBehaviour
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
                    if (_mainCamera.orthographic)
                    {
                        float halfHeight = _mainCamera.orthographicSize;
                        float aspectRatio = (float)Screen.width / (float)Screen.height;
                        float halfWidth = halfHeight * aspectRatio;
                        float camX = _mainCamera.transform.position.x;
                        float camZ = _mainCamera.transform.position.z;
                        float[] screenBotLeft = new float[2] { 0.0f, 0.0f };
                        float[] screenUpRight = new float[2] { 0.0f, 0.0f };
                        screenBotLeft[0] = camX - halfWidth;
                        screenBotLeft[1] = camZ - halfHeight;
                        screenUpRight[0] = camX + halfWidth;
                        screenUpRight[1] = camZ + halfHeight;

                        float x = (_worldPos.x - screenBotLeft[0]) / (halfWidth * 2.0f) * (float)Screen.width;
                        float y = (_worldPos.z - screenBotLeft[1]) / (halfHeight * 2.0f) * (float)Screen.height;
                        _rectTransform.anchoredPosition = new Vector2(x, y);
                    }
                    else
                    {
                        var p = _mainCamera.WorldToScreenPoint(_worldPos);
                        _rectTransform.anchoredPosition = new Vector2(p.x, p.y);
                    }
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
