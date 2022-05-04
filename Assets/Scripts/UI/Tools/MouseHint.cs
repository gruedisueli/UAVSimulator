using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class MouseHint : MonoBehaviour
    {
        public Text _text;
        private RectTransform _rectTransform;
        private bool _active = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("Could not find rect transform on mouse hint");
                return;
            }

            if (_text == null)
            {
                Debug.LogError("Text component of mouse hint not specified");
                return;
            }
        }
        public void Activate(string text)
        {
            if (!EnvironManager.Instance.Environ.SimSettings.DisplayTooltips) return;
            _text.text = text;
            _active = true;
            gameObject.SetActive(true);
            StartCoroutine(FollowMouse());
        }

        public void Deactivate()
        {
            if (!EnvironManager.Instance.Environ.SimSettings.DisplayTooltips) return;
            _active = false;
            gameObject.SetActive(false);
        }

        private IEnumerator FollowMouse()
        {
            while (_active)
            {
                _rectTransform.anchoredPosition = Input.mousePosition + Vector3.up * 50;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
